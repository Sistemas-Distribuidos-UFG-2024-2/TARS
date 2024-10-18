use serde::{Deserialize, Serialize};
use std::net::UdpSocket;

#[derive(Deserialize)]
struct ClientMessage {
    age: u32,
    service_time: u32,
}

#[derive(Serialize)]
struct ServerResponse {
    can_retire: bool,
}

fn main() -> Result<(), Box<dyn std::error::Error>> {
    let socket = UdpSocket::bind("127.0.0.1:8080").expect("Could not bind to address");

    let mut buf = [0; 1024];

    loop {
        let (number_of_bytes, src) = socket.recv_from(&mut buf).expect("Failed to receive data");
        let msg = &buf[..number_of_bytes];

        let client_message: ClientMessage =
            serde_json::from_slice(msg).expect("Failed to parse JSON");

        // Logic to determine if the client can retire
        let can_retire = client_message.age >= 65
            || client_message.service_time >= 30
            || client_message.age >= 60 && client_message.service_time >= 25;

        let response = ServerResponse { can_retire };
        let response_json = serde_json::to_vec(&response).expect("Failed to serialize JSON");

        socket
            .send_to(&response_json, &src)
            .expect("Failed to send response");
    }
}
