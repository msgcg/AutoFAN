Задача создать проект, который бы автоматически управлял вентиляторами в зависимости от того КАКОЙ прибор греется.
Сейчас в проекте есть много ручных и частично ручных настроек, и это сложно, нет профилей управления и тп.
Задача добавить Интеллектуальный режим:
я скинул в docks примеры библиотек для нагрузочного тестирования.
План состоит в следующем: система берет те вентиляторы, которые заранее не привязаны к устройствам (SYS fan(s)) и пытается понять какой где стоит.
Для этого производится попытка нагрузочного тестирования и поочередного поднятия оборотов вентиляторов - какой снизил температуру сильнее - тот и принадлежит устройству.
Так же будет доступна галочка, что некоторые кулера подключены неверно(например CPU или GPU в реальности стоят на дисках и наоборот) - система должна уметь переопределять и такое.
После автоматической мапизации будет доступно несколько автоматических профилей(сейчас они импортируются только как файлы, а не по кнопкам), а так же ручная настройка графика (уже есть как база).
Задача сделать современный, понятный простому пользователю интерфейс.
Так же надо переименовать приложение в AutoFAN.

сейчас не работает:
1) не понятно куда сохраняется профиль маппинга, нет возможности его применить в "автоматическом управлении вентиляторами".
2) Ии ремпап не работает - System.TypeInitializationException
  HResult=0x80131534
  Сообщение = Инициализатор типа "LLama.Native.NativeApi" выдал исключение.
  Источник = LLamaSharp
  Трассировка стека:
   в LLama.Native.NativeApi.llama_max_devices()
   в LLama.Abstractions.TensorSplitsCollection..ctor()
   в LLama.Common.ModelParams..ctor(String modelPath)
   в FanCtrl.IntelligentForm.<OptimizeWithAI>b__8_0() в D:\Cloud\OneDrive - MSFT\Документы\Visual Studio 2022\AutoFAN_diplom\src\UI\IntelligentForm.cs:строка 161
   в System.Threading.Tasks.Task.Execute()

  Изначально это исключение было создано в этом стеке вызовов: 
    [Внешний код]

Внутреннее исключение 1:
RuntimeError: The native library cannot be correctly loaded. It could be one of the following reasons: 
1. No LLamaSharp backend was installed. Please search LLamaSharp.Backend and install one of them. 
2. You are using a device with only CPU but installed cuda backend. Please install cpu backend instead. 
3. One of the dependency of the native library is missed. Please use `ldd` on linux, `dumpbin` on windows and `otool`to check if all the dependency of the native library is satisfied. Generally you could find the libraries under your output folder.
4. Try to compile llama.cpp yourself to generate a libllama library, then use `LLama.Native.NativeLibraryConfig.WithLibrary` to specify it at the very beginning of your code. For more informations about compilation, please refer to LLamaSharp repo on github.
Более того надо убедиться, что в ии модель отправляется не только ручной маппинг, но и данные в процессе измерения - температуры, дельты, обороты, названия датчиков и тп