using HttpServer.Models;
using HttpServer.RouteHandlers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using TrafficWatch.Services.Detail;

namespace TrafficWatch
{
    public class Server
    {
        readonly string Path;
        private Thread thread;
        readonly TrafficWatch.View.Detail.ModelHistory History = new TrafficWatch.View.Detail.ModelHistory();
        readonly HttpServer.HttpServer httpServer;
        public Server()
        {
            log4net.Config.XmlConfigurator.Configure();
            var assembly = Assembly.GetExecutingAssembly();
            Path =System.IO.Path.GetDirectoryName( assembly.Location);
            History.Initialize();
            var route_config = new List<Route>() {
                new Route {
                    Name = "Hello Handler",
                    UrlRegex = @"^/$",
                    Method = "GET",
                    Callable = (HttpRequest request) => {
                        return new HttpResponse()
                        {
                            ContentAsUTF8 = "Hello from Server",
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                     }
                }
                //,new Route()
                //    {
                //        Callable = new FileSystemRouteHandler() { BasePath = @"C:\Documents\GitHub\to-do-notifications"}.Handle,
                //        UrlRegex = "^\\/Static\\/(.*)$",
                //        Method = "GET"
                //    }
                ,
                new Route()
                    {
                        Callable = new FileSystemRouteHandler() { BasePath =Path+ @"\Resources\Pages\"}.Handle,
                        UrlRegex = "^\\/state\\/(.*)$",
                        Method = "GET"
                    },
                new Route {
                    Name = "Download",
                    UrlRegex = @"^/Download",
                    Method = "GET",
                    Callable = (HttpRequest request) => {
                        return new HttpResponse()
                        {
                            ContentAsUTF8 = Tool.ToString(PopWindow.Me.State.download),
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                     }
                },
                new Route {
                    Name = "Upload",
                    UrlRegex = @"^/Upload",
                    Method = "GET",
                    Callable = (HttpRequest request) => {
                        return new HttpResponse()
                        {
                            ContentAsUTF8 = Tool.ToString(PopWindow.Me.State.upload),
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                     }
                },
                new Route {
                    Name = "MaxSpeed",
                    UrlRegex = @"^/MaxSpeed",
                    Method = "GET",
                    Callable = (HttpRequest request) => {
                        return new HttpResponse()
                        {
                            ContentAsUTF8 = History.MaxSpeed,
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                     }
                },
                new Route {
                    Name = "Daydownload",
                    UrlRegex = @"^/Daydownload",
                    Method = "GET",
                    Callable = (HttpRequest request) => {
                        return new HttpResponse()
                        {
                            ContentAsUTF8 = History.Daydownload,
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                     }
                },
                new Route {
                    Name = "Dayupload",
                    UrlRegex = @"^/Dayupload",
                    Method = "GET",
                    Callable = (HttpRequest request) => {
                        return new HttpResponse()
                        {
                            ContentAsUTF8 = History.Dayupload,
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                     }
                },
                new Route {
                    Name = "Monthdownload",
                    UrlRegex = @"^/Monthdownload",
                    Method = "GET",
                    Callable = (HttpRequest request) => {
                        return new HttpResponse()
                        {
                            ContentAsUTF8 = History.Monthdownload,
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                     }
                },
                new Route {
                    Name = "Monthupload",
                    UrlRegex = @"^/Monthupload",
                    Method = "GET",
                    Callable = (HttpRequest request) => {
                        return new HttpResponse()
                        {
                            ContentAsUTF8 = History.Monthupload,
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                     }
                }
                ,
                new Route {
                    Name = "Totaldownload",
                    UrlRegex = @"^/Totaldownload",
                    Method = "GET",
                    Callable = (HttpRequest request) => {
                        return new HttpResponse()
                        {
                            ContentAsUTF8 = History.Totaldownload,
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                     }
                },
                new Route {
                    Name = "Totalupload",
                    UrlRegex = @"^/Totalupload",
                    Method = "GET",
                    Callable = (HttpRequest request) => {
                        return new HttpResponse()
                        {
                            ContentAsUTF8 = History.Totalupload,
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                     }
                }
            };

           httpServer = new HttpServer.HttpServer(8080, route_config);

            Thread = new Thread(new ThreadStart(httpServer.Listen));
            //Thread.Start();
        }
        public void Start()
        {
            Thread.Start();
        }

        [Obsolete]
        public void Suspend()
        {
            Thread.Suspend();
        }
        public void Abort()
        {
            httpServer.Stop();
            Thread.Abort();
        }

        [Obsolete]
        public void Resume()
        {
            Thread.Resume();
        }
        public Thread Thread { get => thread; set => thread = value; }

        public void GetData()
        {
            //History.MaxSpeed
        }
    }
}
