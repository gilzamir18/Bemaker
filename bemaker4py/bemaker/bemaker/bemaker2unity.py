import sys
import socketserver
from .workers import BMWorker
import socket

class BMUDPHandler(socketserver.DatagramRequestHandler):
    worker = BMWorker()
    def handle(self):
        # Receive a message from a client
        msgRecvd = self.rfile.read(None)
        content = BMUDPHandler.worker.proccess(msgRecvd)
        # Send a message from a client
        if  content is not None:
            self.wfile.write(content.encode(encoding="utf-8"))
        else:
            print("WARNING: returning empty message!")
            self.wfile.write("".encode(encoding="utf-8"))

def create_server(agents, ids, server_IP="127.0.0.1", server_port=8080, buffer_size=8192, waittime=0, timeout=10):
    for i in range(len(agents)):
        if not BMWorker.register_agent(agents[i], ids[i], waittime, timeout):
            sys.exit(-1)

    serverAddress   = (server_IP, server_port)
    serverUDP = socketserver.UDPServer(serverAddress, BMUDPHandler)
    serverUDP.max_packet_size = buffer_size
    serverUDP.timeout = timeout
    serverUDP.handle_timeout = lambda : print(".......................TIMEOUT.......................")
    serverUDP.serve_forever()

if __name__ == "__main__":
    print('''
    bemaker2unity it is not an application.
    Use bemaker.appserver.startasdaemon function
    to initialize an controller to control
    an unity application.
    ''')
