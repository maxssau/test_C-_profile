/*
 * Created by SharpDevelop.
 * User: user
 * Date: 28.04.2021
 * Time: 15:31
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 

 
using System;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
//using System.Threading;
using System.Collections.Generic;

namespace bumagi_test
{
	class App
	{
		/// <summary>
		/// Строка для вывода справки по командам на экран
		/// </summary>
		private const string __help=@"
Список доступных команд:

-new_profile - Заполнить новую анкету
-statistics - Показать статистику всех заполненных анкет
-save - Сохранить заполненную анкету
-goto_question <Номер вопроса> - Вернуться к указанному вопросу (Команда доступна только при заполнении анкеты, вводится вместо ответа на любой вопрос)
-goto_prev_question - Вернуться к предыдущему вопросу (Команда доступна только при заполнении анкеты, вводится вместо ответа на любой вопрос)
-restart_profile - Заполнить анкету заново (Команда доступна только при заполнении анкеты, вводится вместо ответа на любой вопрос)
-find <Имя файла анкеты> - Найти анкету и показать данные анкеты в консоль
-delete <Имя файла анкеты> - Удалить указанную анкету
-list - Показать список названий файлов всех сохранённых анкет
-list_today - Показать список названий файлов всех сохранённых анкет, созданных сегодня
-zip <Имя файла анкеты> <Путь для сохранения архива> - Запаковать указанную анкету в архив и сохранить архив по указанному пути
-help - Показать список доступных команд с описанием
-exit - Выйти из приложения
";
		/// <summary>
		/// Строковый массив доступных команд
		/// </summary>
		private string[] strCmdList={
			"-new_profile",
			"-statistics",
			"-save",
			"-goto_question",
			"-goto_prev_question",
			"-restart_profile",
			"-find",
			"-delete",
			"-list",
			"-list_today",
			"-zip",
			"-help",
			"-exit"};
		
		/// <summary>
		/// Строковый массив доступных языков программирования
		/// </summary>
		private string[] strProgLang={
			"PHP", 
			"JavaScript", 
			"C", 
			"C++", 
			"Java", 
			"C#", 
			"Python",
			"Ruby"
		};
		
		/// <summary>
		/// Строковый массив вопросов для анкеты
		/// </summary>
		private string[] strNewProfileLines={
			"ФИО",
			"Дата рождения",
			"Любимый язык программирования",
			"Опыт программирования на указанном языке",
			"Мобильный телефон"
		};
		
		/// <summary>
		/// Регулярные выражения для извлечения данных из файлов анкет.
		/// </summary>
		string[] __reg_ex_parser=
		{
			@"([A-Za-zА-Яа-я-]* [A-Za-zА-Яа-я-]* [A-Za-zА-Яа-я-]*)$",	// ФИО
			@"([0-9]{2}[.|\/][0-9]{2}[.|\/][0-9]{4})$",				//	Дата рождения
			@":([A-Za-z#]+)$",											//	язык программирования
			@":([0-9]{1,2})$",											//	опыт
			@"((\+|([0-9]{0,3}))([0-9]{1,3})(\(|)([0-9]{3})(\)|)([0-9]{3})( |-|)([0-9]{2})( |-|)([0-9]{2}))$"		// номер телефона
		};
		
		/// <summary>
		/// Каталог хранения анкет
		/// </summary>
		private string ProfilePath=".\\Анкеты";
		
		/// <summary>
		/// Временная папка для создания zip архивов. Необходима для упаковки файлов, т.к. класс упаковщика не умеет напрямую создавать файлы, а вместо этого упаковывает все файлы в папке.
		/// </summary>
		private string TempDirectory=".\\temp";
		
		private string __strRegExFileName=@"<([А-ЯЁа-яёA-Za-z0-9 .-:\/\\_]*)>";
		
		private Regex rgxCheckLine;
		private Regex rgxCheckText;
		private Regex rgxCheckDate;
		private Regex rgxCheckNum;
		private Regex rgxCheckSkill;
		private Regex rgxCheckPhone;
			
		private Match mchCheckLine;
		
		Profile structProfile=new Profile();
		
		
		
		/// <summary>
		/// Начальная инициализация переменных
		/// </summary>
		// <param name="arg">Описание параметра</param>
		/// <returns>Void</returns>
		private void InitValues()
		{
			rgxCheckLine=new Regex(@"^[а-яА-ЯёЁa-zA-Z0-9\-\\\.\+\#_ :()<>]+$");
			rgxCheckText=new Regex(@"^([А-ЯЁ][а-яё]{2,}([-][А-ЯЁ][а-яё]{2,})?\s[А-ЯЁ][а-яё]{2,}\s[А-ЯЁ][а-яё]{2,})|([A-Z][a-z]{2,}([-][A-Z][a-z]{2,})?\s[A-Z][a-z]{2,}\s[A-Z][a-z]{2,})");
			rgxCheckDate=new Regex(@"(0[1-9]|[12][0-9]|3[01])[.|\/](0[1-9]|1[012])[.|\/](19|20)\d\d");
			rgxCheckNum=new Regex(@"^[0-9]+$");
			rgxCheckSkill=new Regex("^([0-9]{1,2})$");
			rgxCheckPhone=new Regex(@"^((\+|([0-9]{0,3}))([0-9]{1,3})(\(|)([0-9]{3})(\)|)([0-9]{3})( |-|)([0-9]{2})( |-|)([0-9]{2}))$");
		}
		
		
		
		/// <summary>
		/// Конструктор класса
		/// </summary>
		// <param name="arg">Описание параметра</param>
		/// <returns>Void</returns>
		public App()
		{
			InitValues();
		}
		
		
		
		/// <summary>
		/// Проверка команды на правильность ввода
		/// </summary>
		/// <param name="cmd">Проверяемое слово</param>
		/// <returns>Индекс в строковом массиве. В случае неверной команды, возвращается -1</returns>
		private int CmdCheckCommand(string cmd)
		{
			// проверяем строку на наличие команды
			for(int i=0;i<strCmdList.Length;i++)
			{
				if(strCmdList[i]==cmd)
				{
					return i;
				}
			}
			// ничего не нашлось
			return -1;
		}
		
		
		/// <summary>
		/// Структура для хранения анкеты.
		/// DataEntered(int32) - битовая маска для хранения статуса заполнения.
		/// DataReady(bool) - признак правильности заполнения всех полей анкеты.
		/// </summary>
		private struct Profile
		{
			public string FIO;
			public string DateBD;
			public string Lang;
			public int Skill;
			public string MobilePh;
			public bool DataReady;
			public int DataEntered;
		}
		
		/// <summary>
		/// Структура для хранения данных при подсчете статистики.
		/// </summary>
		private struct ProfileStat
		{
			public string FIO;
			public int Skill;
			public int Lang;	// id языка программирования
			public int DateYO;	// возраст, лет
		}
		
		
		/// <summary>
		/// Проверка правильности ввода названия языка программирования.
		/// </summary>
		/// <param name="cmd">Проверяемое слово. Строка должна быть предварительно подготовлена с помощью регулярного выражения.
		/// Допустимые языки: PHP, JavaScript, C, C++, Java, C#, Python, Ruby.
		/// Регистр символов значения не имеет.
		/// </param>
		/// <returns>Индекс в массиве названий языков программирования</returns>
		private int CmdNewProfileLangCheck(string cmd)
		{
			for(int i=0;i<strProgLang.Length;i++)
			{
				// переврдим все символы к строчным
				if(strProgLang[i].ToLower()==cmd.ToLower())
				{
					// если  строки совпадают, возвращаем индекс
					return i;
				}
			}
			return -1;
		}
		
		
		/// <summary>
		/// Команда -list
		/// Процедура вывода названий файлов анкет в консоль. Файлы анкет расположены в каталоге Анкеты.
		/// Реализованный вариант - попробовать парсить каждый найденный файл и выводить название только если файл имеет верный формат.
		/// Главный минус - скорость немного падает, но зато выводятся только валидные файлы
		/// 
		/// </summary>
		private void CmdListFiles()
		{
			
			// показать список файлов
			try
			{
				// проверка существования каталога
				int CheckExistDirStatus=CheckSaveDir();
				switch(CheckExistDirStatus)
				{
					case 0:
					{
						// директория не существует, создаём 
						Directory.CreateDirectory("Анкеты");
					}
					break;
					case 1:
					{
						// директория существует
						break;
					}
				}
				// получаем список файлов
				string[] strFiles=Directory.GetFiles(ProfilePath,"*.txt");
				Console.WriteLine("\nСписок доступных анкет:\n");
				
				// выводим на экран названия файлов
				for(int i=0;i<strFiles.Length;i++)
				{
					// выводим название только валидных файлов
					if(ParseProfile(Path.GetFileName(strFiles[i])))
					{
						Console.WriteLine(Path.GetFileName(strFiles[i]));
					}
					
					//Console.WriteLine(Path.GetFileName(strFiles[i]));
				}
				Console.WriteLine("\n");
				structProfile.DataReady=false;
			}
			catch(Exception ex)
			{
				structProfile.DataReady=false;
				Console.WriteLine("\nОшибка вывода списка файлов: "+ex.Message);
			}
			
			
			
		}
		
		/// <summary>
		/// Команда -list_today
		/// Вывод списка файлов на экран, созданнх в текущий день работы. Файлы расположены в каталоге Анкеты.
		/// Реализованный вариант - попробовать парсить каждый найденный файл и выводить название только если файл имеет верный формат.
		/// Главный минус - скорость немного падает, но зато выводятся только валидные файлы
		/// </summary>
		private void CmdListFilesToday()
		{
			// показать список файлов за сегодня
			try
			{
				// проверка существования каталога
				int CheckExistDirStatus=CheckSaveDir();
				switch(CheckExistDirStatus)
				{
					case 0:
					{
						// директория не существует, создаём 
						Directory.CreateDirectory("Анкеты");
					}
					break;
					case 1:
					{
						// директория существует
						break;
					}
				}
				// получаем список файлов
				string[] strFiles=Directory.GetFiles(ProfilePath,"*.txt");
				
				Console.WriteLine("\nСписок анкет, созданных сегодня:\n");
				DateTime dtFileDate;
				
				for(int i=0;i<strFiles.Length;i++)
				{
					
					// получаем время создания файла
					dtFileDate=File.GetCreationTime(strFiles[i]);
						
					// если число, месяц и год совпадают с текущим днём, то выводим на экран
					if(dtFileDate.Day==DateTime.Now.Day && dtFileDate.Month==DateTime.Now.Month && dtFileDate.Year==DateTime.Now.Year)
					{
						// если файл прошел валидацию, выводим название на экран
						if(ParseProfile(Path.GetFileName(strFiles[i])))
						{
							Console.WriteLine(Path.GetFileName(strFiles[i]));
						}
					}
					
				}
				Console.WriteLine("\n");
				structProfile.DataReady=false;
			}
			catch(Exception ex)
			{
				structProfile.DataReady=false;
				Console.WriteLine("\nОшибка вывода списка файлов: "+ex.Message);
			}
		}
		
		/// <summary>
		/// Команда -find
		/// Вывод информации из файла анкеты на экран.
		/// </summary>
		/// <param name="filename">Название файла без пути</param>
		private void CmdFindProfile(string filename)
		{
			// проверка на наличие файла в каталоге
			if(File.Exists(ProfilePath+"\\"+filename)==true)
			{
				// если профиль успешно распознан, выводим содержимое на экран
				if(ParseProfile(filename)==true)
				{
					Console.WriteLine("\n");
					Console.WriteLine(strNewProfileLines[0]+":"+structProfile.FIO);
					Console.WriteLine(strNewProfileLines[1]+":"+structProfile.DateBD);
					Console.WriteLine(strNewProfileLines[2]+":"+structProfile.Lang);
					Console.WriteLine(strNewProfileLines[3]+":"+structProfile.Skill.ToString());
					Console.WriteLine(strNewProfileLines[4]+":"+structProfile.MobilePh);
					Console.WriteLine("\n");
				}
				structProfile.DataReady=false;
			}
			else
			{
				structProfile.DataReady=false;
				Console.WriteLine("\nНазвание файла указано неверно\n");
			}
		}
		
		/// <summary>
		/// Разбор информации в строках текстового файла и сохранение в структуре анкеты.
		/// </summary>
		/// <param name="filename">Название файла без пути</param>
		/// <returns>false - если файл не разобран или произошло исключение
		/// true - если чтение и разбор прошло успешно</returns>
		private bool ParseProfile(string filename)
		{
			string[] __file;
			try
			{
				/* Читаем полностью файл по строкам.
				 * По хорошему, здесь не помешало бы проверить файл на разумный размер 
				 * и на содержимое (на случай если он бинарный и нам явно не подходит), но в ТЗ про это ничего не сказано.
				*/
				__file=File.ReadAllLines(ProfilePath+"\\"+filename);
				// пытаемся распознать данные в строках
				if(__file.Length>5)
				{
					// если в прочитанном файле больше 5 строк, то продолжаем
					// и обрабатываем только 5 строк
					
					Regex __reg;
					
					structProfile=new Profile();
					structProfile.DataEntered=0;
					
					for(int i=0;i<5;i++)
					{
						for(int j=0;j<5;j++)
						{
							// перебираем строки и смотрим на совпадения
							__reg=new Regex(__reg_ex_parser[j]);
							MatchCollection __matches=__reg.Matches(__file[i]);
							if(__matches.Count>0)
							{
								// что-то совпало, извлекаем
								switch(j)
								{
									case 0:
										{
											// извлекаем фамилию
											structProfile.FIO=__matches[0].Groups[0].Value;
											structProfile.DataEntered=structProfile.DataEntered | 1<<0;
										}
										break;
									case 1:
										{
											// извлекаем дату рождения
											structProfile.DateBD=__matches[0].Groups[0].Value;
											structProfile.DataEntered=structProfile.DataEntered | 1<<1;
										}
										break;
									case 2:
										{
											// извлекаем язык программирования
											structProfile.Lang=__matches[0].Groups[0].Value.Substring(1);
											structProfile.DataEntered=structProfile.DataEntered | 1<<2;
										}
										break;
									case 3:
										{
											// извлекаем уровень опыта
											structProfile.Skill=Convert.ToInt32(__matches[0].Groups[0].Value.Replace(':',' '));
											structProfile.DataEntered=structProfile.DataEntered | 1<<3;
										}
										break;
									case 4:
										{
											// извлекаем номер телефона
											structProfile.MobilePh=__matches[0].Groups[0].Value;
											structProfile.DataEntered=structProfile.DataEntered | 1<<4;
										}
										break;
								}
							}
						}
					}
					
					int first_unset=-1;
					
					// если все строки заполнены, то возвращаем true
					for(int i=0;i<5;i++)
					{
						if((structProfile.DataEntered & 1<<i)==0)
						{
							if(first_unset==-1)
							{
								first_unset=i;
							}
							Console.WriteLine("\nДанные \"" + strNewProfileLines[i] + "\" не прочитаны из файла профиля");
						}
					}
					
					if(first_unset==-1)
					{
						structProfile.DataReady=true;
						return true;
						
					}
					else
					{
						return false;
					}
					
				}
				else
				{
					//Console.WriteLine("\nОшибка чтения файла, файл не является файлом анкеты\n");
					return false;
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine("\nОшибка чтения файла: "+ex.Message);
				return false;
			}
		}
		
		/// <summary>
		/// Функция вычисления окончания для вывода кол-ва лет на экран. Протестировано до 150, т.к. больше этого значения нет необходимости
		/// </summary>
		/// <param name="val">Целое число лет для вычисления окончания</param>
		/// <returns>Окончание в случае успеха и пустая строка, если число меньше нуля.</returns>
		private string GetYearTail(int val)
		{
			if(val<0)
			{
				return "";
			}
			
			// для анализа берется остаток от деления на 10
			int __tmp=val%10;
			string tail="";
			if(__tmp==0)
			{
				tail="лет";
			};
			if(__tmp==1)
			{
				tail="год";
			};
			if(__tmp>1 && __tmp<5)
			{
				tail="года";
			};
			if(__tmp>4)
			{
				tail="лет";
			}
			
			// есть исключения для 11,12,13,14 лет
			if(val>10 & val<15)
			{
				tail="лет";
			}
			
			return tail;
		}
		
		/// <summary>
		/// Команда -statistics
		/// Процедура вывода статистики на экран.
		/// </summary>
		private void CmdStat()
		{
			// читаем список файлов в папке анкеты
			try
			{
				// читаем список файлов
				string[] strFiles=Directory.GetFiles(ProfilePath,"*.txt");
				List <ProfileStat> __prof=new List<ProfileStat>();
				for(int i=0;i<strFiles.Length;i++)
				{
					// перебираем все файлы и заполняем список
					if(ParseProfile(Path.GetFileName(strFiles[i])))
					{
						// если файл успешно прочитан, заполняем элемент списка
						ProfileStat __prof_item=new ProfileStat();
						__prof_item.FIO=structProfile.FIO;
						__prof_item.DateYO=DateTime.Now.Year-DateTime.Parse(structProfile.DateBD).Year;
						__prof_item.Skill=structProfile.Skill;
						__prof_item.Lang=CmdNewProfileLangCheck(structProfile.Lang);
						__prof.Add(__prof_item);
					}
				}
				
				// список заполнен, вычисляем статистику
				int summ=0;
				int[] __lang=new int[strProgLang.Length];
				int max_skill=0;
				int max_skill_index=0;
				int popular_lang=0;
				int popular_lang_index=0;
				
				for(int i=0;i<__prof.Count;i++)
				{
					// средний возраст
					summ=summ+__prof[i].DateYO;
					
					// популярный язык
					__lang[__prof[i].Lang]++;
					
					// опытный программист
					if(max_skill<__prof[i].Skill)
					{
						max_skill=__prof[i].Skill;
						max_skill_index=i;
					}
				}
				
				
				// поиск самого популярного языка
				for(int i=0;i<strProgLang.Length;i++)
				{
					if(__lang[i]>popular_lang)
					{
						popular_lang=__lang[i];
						popular_lang_index=i;
					}
				}
				
				int __med=summ/__prof.Count;
				string strTail=GetYearTail(__med);
				Console.WriteLine("\nСтатистика данных анкет:\n");
				// вывод информации на экран
				Console.WriteLine("Средний возраст:" + __med.ToString()+" "+strTail);
				Console.WriteLine("Самый популярный язык программирвания:" + strProgLang[popular_lang_index]);
				Console.WriteLine("Самый опытный программист:"+__prof[max_skill_index].FIO);
				Console.WriteLine("\n");
				structProfile.DataReady=false;
			}
			catch(Exception ex)
			{
				Console.WriteLine("\nОшибка подсчета статистики: "+ex.Message);
			}
		}
		
		/// <summary>
		/// Команда -zip
		/// Сохранение файла анкеты в zip архиве по заданному пути.
		/// </summary>
		/// <param name="filename">Название файла анкеты без пути</param>
		/// <param name="path">Путь к целевому каталогу сохранения архивов</param>
		private void CmdZipFile(string filename, string path)
		{
			try
			{
				//проверяем валидность пути файла
				if(File.Exists(ProfilePath+"\\"+filename)==true)
				{
					// файл найден
					// проверяем целевую папку на валидность
					if(Directory.Exists(path)==true)
					{
						// директория найдена, пробуем сохранить
						// проверяем, есть ли уже упакованный файл с таким именем
						// создаём временную папку
						
						if(!Directory.Exists(TempDirectory))
						{
							Directory.CreateDirectory(TempDirectory);
						}
						else
						{
							// очищаем директорию
							DirectoryInfo dirInfo = new DirectoryInfo(TempDirectory);
 
							foreach (FileInfo file in dirInfo.GetFiles())
							{
    							file.Delete();
							}
						}
						
						
						// копируем необходимый файл во временную папку
						File.Copy(ProfilePath+"\\"+filename,TempDirectory+"\\"+filename);
						
						// т.к. в ТЗ не указано что делать с существующими файлами, то просто подтираем
						if(File.Exists(path+"\\"+filename+".zip"))
						{
							File.Delete(path+"\\"+filename+".zip");
						}
						// сжимаем файл в zip архив
						ZipFile.CreateFromDirectory(TempDirectory,path+"\\"+filename+".zip");
						// подчищаем за собой
						File.Delete(TempDirectory+"\\"+filename);
						
					}
					else
					{
						// вкаралась очепятка в целевой путь
						Console.WriteLine("\nОшибка ввода целевой директории\n");
					}
				}
				else
				{
					// файл не найден
					Console.WriteLine("\nОшибка ввода названия файла\n");
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine("\nОшибка:"+ex.Message+"\n"+ex.StackTrace);
			}
		}
		
		
		/// <summary>
		/// Команда -new_profile
		/// Создание анкеты
		/// </summary>
		private void CmdNewProfile()
		{
			int current_question=1;
			
			// создаём новый профиль
			structProfile=new Profile();
			structProfile.DataReady=false;
			structProfile.DataEntered=0;
			
			Console.WriteLine("\nВведите данные анкеты:\n");
			
			while(true)
			{
				Console.WriteLine(strNewProfileLines[current_question-1]);
				                 
				// читаем строку из консоли и делим её на слова
				string cmd_input=Console.ReadLine();
				string[] strWords=cmd_input.Split();

				// проверка на доп. 3 команды
				if(CmdCheckCommand(strWords[0])==3)
				{
					// команда -goto_question
					if(strWords.Length==2)
					{
						// проверяем второй аргумент, что это число в диапазоне 1-5
						mchCheckLine=rgxCheckNum.Match(strWords[1]);
						if(mchCheckLine.Success==true)
						{
							int _cmd=Convert.ToInt32(strWords[1]);
							//Console.WriteLine("Selected line:"+_cmd.ToString());
							if(_cmd>1 & _cmd<6)
							{
								current_question=_cmd;
								continue;
							}
							else
							{
								Console.WriteLine("\nОшибка ввода команды -goto_question. Аргумент должен быть числом от 1 до 5\n");
								continue;
							}
						}
						else
						{
							Console.WriteLine("\nОшибка ввода команды -goto_question. Аргумент должен быть числом от 1 до 5\n");
							continue;
						}
					}
					else
					{
						Console.WriteLine("\nОшибка ввода команды -goto_question. Аргумент должен быть числом от 1 до 5\n\n");
					}
					
				}
				else if(CmdCheckCommand(strWords[0])==4)
				{
					// команда -goto_prev_question
					if(current_question!=1)
					{
						current_question--;
						continue;
					}
				}
				else if(CmdCheckCommand(strWords[0])==5)
				{
					// команда -restart_profile
					current_question=1;
					structProfile.DataEntered=0;
					structProfile.DataReady=false;;
					continue;
				}
				
				// парсинг строк согласно номеру вопроса
				switch(current_question)
				{
					case 1:
						{
							// ввод ФИО
							mchCheckLine=rgxCheckText.Match(cmd_input);
							if(mchCheckLine.Success==true)
							{
								structProfile.FIO=cmd_input;
								current_question++;
								structProfile.DataEntered = structProfile.DataEntered | 1;
							}
							else
							{
								Console.WriteLine("\nОшибка ввода ФИО\n");
							}
							continue;
						}
					case 2:
						{
							// дата
							mchCheckLine=rgxCheckDate.Match(cmd_input);
							if(mchCheckLine.Success==true)
							{
								structProfile.DateBD=DateTime.Parse(cmd_input).ToString("dd/MM/yyyy");
								structProfile.DataEntered = structProfile.DataEntered | 1<<1;
								current_question++;
							}
							else
							{
								Console.WriteLine("\nОшибка ввода даты. Формат ввода даты: ДД.ММ.ГГГГ\n");
							}
							continue;
						}
					case 3:
						{
							// язык программирования
							int intProgLang=CmdNewProfileLangCheck(cmd_input);
							if(intProgLang!=-1)
							{
								structProfile.Lang=strProgLang[intProgLang];
								structProfile.DataEntered = structProfile.DataEntered | 1<<2;
								current_question++;
							}
							else
							{
								Console.WriteLine("\nОшибка ввода языка программирования. Допустимые значения: PHP, JavaScript, C, C++, Java, C#, Python, Ruby\n");
							}
							continue;
						}
					case 4:
						{
							// skill
							mchCheckLine=rgxCheckSkill.Match(cmd_input);
							if(mchCheckLine.Success==true)
							{
								structProfile.Skill=Convert.ToInt32(cmd_input);
								structProfile.DataEntered = structProfile.DataEntered | 1<<3;
								current_question++;
								continue;
							}
							else
							{
								Console.WriteLine("\nОшибка ввода опыта работы. Допустимое число от 0 до 99\n");
							}
						}
						break;
					case 5:
						{
							// номер телефона
							mchCheckLine=rgxCheckPhone.Match(cmd_input);
							if(mchCheckLine.Success==true)
							{
								structProfile.MobilePh=cmd_input;
								structProfile.DataEntered = structProfile.DataEntered | 1<<4;
								current_question++;
							}
							else
							{
								Console.WriteLine("\nОшибка ввода номера телефона.Предпочтительный формат: Х(ХХХ)ХХХХХХХ\n");
							}
						}
						break;
				};
				
				if(current_question>5)
				{
					// окончание ввода данных анкеты
					// проверка, все ли данные введены, если нет, возвращаемся к первому не введенному элементу
					int first_unset=-1;
					
					for(int i=0;i<5;i++)
					{
						if((structProfile.DataEntered & 1<<i)==0)
						{
							if(first_unset==-1)
							{
								first_unset=i;
							}
							Console.WriteLine("\nДанные \"" + strNewProfileLines[i] + "\" не введены\n");
						}
					}
					if(first_unset!=-1)
					{
						current_question=first_unset+1;
						continue;
					}
					structProfile.DataReady=true;
					break;
				}
			}
		}
		
		/// <summary>
		/// Проверка наличия каталога по заданному пути
		/// </summary>
		/// <returns>0 если каталог не найден, 1 в случае если каталог существует.</returns>
		private int CheckSaveDir()
		{
			try
			{
				if(Directory.Exists(ProfilePath))
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine("\nОшибка: "+ex.Message);
				return -1;
			}
		}
		
		/// <summary>
		/// Команда -save
		/// Сохранение анкеты в файле. Каталог сохранения - Анкеты
		/// </summary>
		private void CmdSaveProfile()
		{
			try
			{
				if(structProfile.DataReady==true)
				{
					// проверка существования каталога
					int CheckExistDirStatus=CheckSaveDir();
					switch(CheckExistDirStatus)
					{
						case -1:
						{
							Console.WriteLine("\nФайл анкеты НЕ записан!\n");
							return;
						}
						case 0:
						{
							// директория не существует, создаём 
							Directory.CreateDirectory("Анкеты");
						}
						break;
						case 1:
						{
							// директория существует
							break;
						}
					}
					// если нет исключения, то директория создана
					string strFileName;
					int file_index=0;
					do
					{
						/*
						 * Пытаемся создать название файла из ФИО, если такой уже есть, то добавляем цифру в конце
						 * Если цифра больше 1к, останавливаемся и выводим ошибку. Т.к. в шаблоне рег. выражения
						 * только рус./анг. буквы, дополнительных проверок можно не делать. Если название больше 240 символов, 
						 * показываем ошибку и выходим (на случай FAT32/NTFS всё должно работать, для FAT16/12 и прочие FS не поддерживающие 255 
						 * символов в названии файла не рассматриваем, т.к. нет в ТЗ).
						 * */
						
						if(file_index==0)
						{
							strFileName=structProfile.FIO +".txt";
						}
						else
						{
							strFileName=structProfile.FIO+"("+file_index.ToString()+").txt";
						}
						file_index++;
						if(file_index>1000)
						{
							Console.WriteLine("\nНевозможно создать файл с индексом более 1000\nФайл НЕ сохранён\n");
							return;
						}
						if(strFileName.Length>240)
						{
							Console.WriteLine("\nНевозможно создать файл, название более 240 символов\nФайл НЕ сохранён\n");
							return;
						}
					}
					while(File.Exists(ProfilePath + "\\"+strFileName));
					
					// запись в файл
					using(StreamWriter __writer=new StreamWriter(ProfilePath+"\\"+strFileName))
					{
						// поочередно сохраняем все поля
						__writer.WriteLine(strNewProfileLines[0]+":"+structProfile.FIO);
						__writer.WriteLine(strNewProfileLines[1]+":"+structProfile.DateBD.ToString());
						__writer.WriteLine(strNewProfileLines[2]+":"+structProfile.Lang);
						__writer.WriteLine(strNewProfileLines[3]+":"+structProfile.Skill.ToString());
						__writer.WriteLine(strNewProfileLines[4]+":"+structProfile.MobilePh);
						
						// добавляем штамп времени
						string strTimeStamp=DateTime.Now.ToString();
						__writer.WriteLine("\nАнкета заполнена: "+strTimeStamp);
					};
					
				}
				else
				{
					Console.WriteLine("\nДанные анкеты не заполнены\n");
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine("\nОшибка:"+ex.Message);
				return;
			}
			
		}
		
		/// <summary>
		/// Команда -delete
		/// Удаление файла анкеты.
		/// </summary>
		/// <param name="filename">Название файла без пути</param>
		private void CmdDeleteFile(string filename)
		{
			try
			{
				if(File.Exists(ProfilePath+"\\"+filename))
				{
					File.Delete(ProfilePath+"\\"+filename);
				}
				else
				{
					Console.WriteLine("\nОшибка в имени файла\n");
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine("\nОшибка:"+ex.Message);
			}
		}
		
		/// <summary>
		/// Глваная функция приложения.
		/// </summary>
		public void Run()
		{
			string strInputCmd="";
			
			// очистка экрана
			Console.Clear();
			
			Console.WriteLine("Выберите действие: \n");
			//Console.WriteLine(__help);
						
			while(true)
			{
				// main loop
				Console.Write("cmd:");
				strInputCmd=Console.ReadLine();
				
				// проверяем введенную строку на валидность
				if(strInputCmd.Length>0)
				{
					// пользователь что-то ввёл в консоли
					mchCheckLine=rgxCheckLine.Match(strInputCmd);
					if (mchCheckLine.Success==true)
					{
						// строка соответствует символам, цифрам и разрешенным спец символам
						string[] strWords=strInputCmd.Split();
						// проверяем введенную команду
						
						switch(CmdCheckCommand(strWords[0]))
						{
							case -1:
								// ошибка ввода, команда не распознана
								{
									Console.Write("\nНераспознанная команда\n");
								}
								break;
							case 0:
								{
									// команда должна содержать только 1 "слово"
									if(strWords.Length==1)
									{
										// -new_profile
										CmdNewProfile();
										Console.WriteLine("Выберите действие: \n");
									}
								}
								break;
							case 1:
								{
									if(strWords.Length==1)
									{
										// -statistics
										CmdStat();
									}
									else
									{
										Console.WriteLine("\nОшибка ввода команды -statistics\n");
									}
								}
								break;
							case 2:
								{
									if(strWords.Length==1)
									{
										// -save
										CmdSaveProfile();
									}
									else
									{
										Console.WriteLine("\nОшибка ввода команды -save\n");
									}
								}
								break;
							case 3:
								{
									// -goto_question
								}
								break;
							case 4:
								{
									// goto_prev_question
								}
								break;
							case 5:
								{
									// -restart_profile
								}
								break;
							case 6:
								{
									// -find
									if(strWords.Length>1)
									{
										Regex __reg=new Regex(__strRegExFileName);
										Match __match=__reg.Match(strInputCmd);
										if(__match.Success)
										{
											CmdFindProfile(__match.Groups[1].Value);
										}
									}
									else
									{
										Console.WriteLine("\nОшибка ввода команды -find. формат ввода команды: -find <имя файла>\n");
									}
								}
								break;
							case 7:
								{
									// -delete
									if(strWords.Length>1)
									{
										Regex __reg=new Regex(__strRegExFileName);
										Match __match=__reg.Match(strInputCmd);
										if(__match.Success)
										{
											CmdDeleteFile(__match.Groups[1].Value);
										}
										else
										{
											Console.WriteLine("\nОшибка ввода команды -delete. Формат команды: -delete <название файла>\n");
										}
									}
									else
									{
										Console.WriteLine("\nОшибка ввода команды -delete. Формат команды: -delete <название файла>\n");
									}
								}
								break;
							case 8:
								{
									if(strWords.Length==1)
									{
									 	// -list
									 	CmdListFiles();
									}
									else
									{
										Console.WriteLine("\nОшибка ввода команды -list\n");
									}
								}
								break;
							case 9:
								{
									if(strWords.Length==1)
									{
										// -list_today
										CmdListFilesToday();
									}
									else
									{
										Console.WriteLine("\nОшибка ввода команды -list_today\n");
									}
								}
								break;
							case 10:
								{
									// -zip
									if(strWords.Length>2)
									{
										Regex __reg=new Regex(__strRegExFileName);
										MatchCollection __match=__reg.Matches(strInputCmd);
										if(__match.Count==2)
										{
											if(__match[0].Success && __match[1].Success)
											{
												CmdZipFile(__match[0].Groups[1].Value,__match[1].Groups[1].Value);
											}
										}
										else
										{
											Console.WriteLine("\nОшибка ввода команды -zip. Формат команды: -zip <имя_файла.txt> <disk:\\путь_к_папке_сохранения>\n");
										}
									}
									else
									{
										Console.WriteLine("\nОшибка ввода команды -zip. Формат команды: -zip <имя_файла.txt> <disk:\\путь_к_папке_сохранения>\n");
									}
								}
								break;
							case 11:
								{
									if(strWords.Length==1)
									{
										// -help
										Console.Clear();
										Console.WriteLine('\n'+__help);
									}
									else
									{
										Console.WriteLine("\nОшибка ввода команды -help\n");
									}
								}
								break;
							case 12:
								{
									if(strWords.Length==1)
									{
										// -exit
										Console.Write("\nВыход из программы, good buy!\n");
										Console.ReadKey(true);
										Environment.Exit(0);
									}
									else
									{
										Console.WriteLine("\nОшибка ввода команды -exit\n");
									}
								}
								break;
							default:
								{
									Console.Write("\nНераспознанная команда\n");
								}
								break;
						}
					}
					else
					{
						Console.Write("\nКоманда не распознана, повторите ввод\n");
					}
				}
			}
		}
		
	}
	
	class Program
	{
		public static void Main(string[] args)
		{
			App __application=new App();
			__application.Run();
		}
	}
}