using HttpServer.Models;
using HttpServer.RouteHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ServerTrafice
{
    public class Server
    { string Path;
        private Thread thread;
        readonly TrafficWatch.View.Detail.ModelHistory History = new TrafficWatch.View.Detail.ModelHistory();
        public Server()
        {
            log4net.Config.XmlConfigurator.Configure();
            var assembly = Assembly.GetExecutingAssembly();
            Path =System.IO.Path.GetDirectoryName( assembly.Location);
            var route_config = new List<Route>() {
                new Route {
                    Name = "Hello Handler",
                    UrlRegex = @"^/$",
                    Method = "GET",
                    Callable = (HttpRequest request) => {
                        return new HttpResponse()
                        {
                            ContentAsUTF8 = "Hello from SimpleHttpServer",
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
                        Callable = new FileSystemRouteHandler() { BasePath =Path+ @"\www\"}.Handle,
                        UrlRegex = "^\\/Static\\/(.*)$",
                        Method = "GET"
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

            HttpServer.HttpServer httpServer = new HttpServer.HttpServer(8080, route_config);

            Thread = new Thread(new ThreadStart(httpServer.Listen));
            Thread.Start();
        }

        public Thread Thread { get => thread; set => thread = value; }

        public void GetData()
        {
            //History.MaxSpeed
        }
    }
}
