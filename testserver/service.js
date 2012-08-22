var http = require('http');
//Quick and dirty web service to test the infinity engine
http.createServer(function (request, response) {
    var names = new Array();
    if(request.url == "/names" || request.url == "/names/")
    {
            names[0] = {
                          "id"        : 123,
                          "firstname" : "bob",
                          "lastname"  : "jones"
                       };
            names[1] = {
                          "id"        : 567,
                          "firstname" : "stephen",
                          "lastname"  : "watters"
                       };
    }
    else if(request.url == "/names/stephen" || request.url == "/names/stephen/")
    {
            names[0] = {
                         "id"        : 567,
                         "firstname" : "stephen",
                         "lastname"  : "smith"
                        };
    }
    response.writeHead(200, {'Content-Type': 'application/json'});
    response.end(JSON.stringify(names));
}).listen(8124);

console.log('Server running at http://127.0.0.1:8124/');
