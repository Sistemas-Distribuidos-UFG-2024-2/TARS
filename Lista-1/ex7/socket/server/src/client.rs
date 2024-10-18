use serde::{Deserialize, Serialize};
use std::net::UdpSocket;

#[derive(Serialize)]
struct ClientMessage {
    age: u32,
    service_time: u32,
}

#[derive(Deserialize, Debug)]
struct ServerResponse {
    can_retire: bool,
}

fn main() -> Result<(), Box<dyn std::error::Error>> {
    let socket = UdpSocket::bind("127.0.0.1:8081").expect("Could not bind to address");

    let request = ClientMessage{
        age: 65,
        service_time: 20
    };

    let request_json = serde_json::to_string(&request).expect("Could not serialize request");

    socket.send_to(request_json.as_bytes(), "127.0.0.1:8080").expect("Could not send request");

    println!("Sent: {}", request_json);

    let mut buf = [0u8; 1024];
    let (len, _) = socket.recv_from(&mut buf).expect("Didn't receive");
    let response_json = String::from_utf8_lossy(&buf[..len]);

    // Parse the JSON response
    let response: ServerResponse = serde_json::from_str(&response_json)?;

    // Output the response
    println!("Received: {:?}", response);

    // Check if the user can retire
    if response.can_retire {
        println!("You can retire!");
    } else {
        println!("You cannot retire yet.");
    }

    Ok(())
}
