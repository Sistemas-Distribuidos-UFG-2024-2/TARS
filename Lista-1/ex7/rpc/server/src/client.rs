use retirement::retirement_client::RetirementClient;
use retirement::RetirementRequest;

pub mod retirement {
    tonic::include_proto!("retirement");
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let mut client = RetirementClient::connect("http://[::1]:50051").await?;

    let request = tonic::Request::new(RetirementRequest {
        age: 65,
        service_time: 12,
    });

    let response = client.can_retire(request).await?;

    println!("RESPONSE={:?}", response);

    Ok(())
}
