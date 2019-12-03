Chat Service

- chatService.sln

         -server.cproj

         -client.cproj 

- A public github repository for the project from the start of the project (I would like to see all the commit histories) 

- Console c# application for the server  

- Console c# application for the clients (should be tested with Minimum 2 clients )

- using only pure .Net asynchronous  socket programming 

- Unit tests are mandatory 

- if one client try to send more than 1 message per second the chat server will warn the client and if this happen again the chat server will close the chat client's connection 

- well structure coding , commenting and usage of necessary design patterns are very important  


Development Details
-------------------
.NET Framework 4.7.2 based
Logging with NLog 
Dependency Injection with Autofac
IDE Visual Studio 2019 Pro

Missing Parts
--------------
No Unit test integrated
unable to be disconnected client(s)
Unable to send messages