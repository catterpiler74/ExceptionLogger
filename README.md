
.NET exception logger output window
It happens to the best of us, our code throws an unexpected exception. This little class will catch any unhandled exceptions in a WinForms app and log them to a text file, event log or website, along with some useful information such as the call stack and loaded DLLs. When logging to a text file, it also ensures the file doesn't get too big.

To use it add the following code to your Windows Forms app, before the call to Application.Run().

ExceptionLogger logger = new ExceptionLogger();
You then need to add one or more of the following logger implementations to do the logging via the ExceptionLogger.AddLogger() method

Available loggers are

TextFileLogger - Logs to a text file
EventLogLogger - Logs to the application event log
EmailLogger - Sends an email with the exception details
WebsiteLogger - Posts the exception details to a web page
WindowLogger - Display the exception details in a window
The class can also be used in console apps. It will behave differently because although unhandled exceptions will be logged, the exception won't be caught so your application will still crash. One way around this is to add a global try..catch block and call LogException() in the catch block. It may also be used in ASP.NET applications although depending on the trust level your code is running in exceptions may be thrown. You are probably better off with the wonderful Elmah logging library.
copy from : https://www.doogal.co.uk/exception
