To use this template with Windows Azure authentication, refer to http://go.microsoft.com/fwlink/?LinkID=267940.

Otherwise, to use this template with Windows authentication, refer to the instructions below:

Hosting on IIS Express:
1. Click on your project in the Solution Explorer to select the project.
2. If the Properties pane is not open, open it (F4).
3. In the Properties pane for your project:
   a) Set "Anonymous Authentication" to "Disabled".
   b) Set "Windows Authentication" to "Enabled".

Hosting on IIS 7 or later:
1. Open IIS Manager and navigate to your website.
2. In Features View, double-click Authentication.
3. On the Authentication page, select Windows authentication. If Windows
   authentication is not an option, you'll need to make sure Windows authentication
   is installed on the server.

      To enable Windows authentication on Windows:
      a) In Control Panel open "Programs and Features".
      b) Select "Turn Windows features on or off".
      c) Navigate to Internet Information Services > World Wide Web Services > Security
         and make sure the Windows authentication node is checked.

      To enable Windows authentication on Windows Server:
      a) In Server Manager, select Web Server (IIS) and click Add Role Services.
      b) Navigate to Web Server > Security
         and make sure the Windows authentication node is checked.

4. In the Actions pane, click Enable to use Windows authentication.
5. On the Authentication page, select Anonymous authentication.
6. In the Actions pane, click Disable to disable anonymous authentication.
