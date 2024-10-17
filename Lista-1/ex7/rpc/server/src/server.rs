use tonic::{transport::Server, Request, Response, Status};

use retirement::retirement_server::{Retirement, RetirementServer};
use retirement::{RetirementRequest, RetirementResponse};

pub mod retirement {
    tonic::include_proto!("retirement");
}

#[derive(Debug, Default)]
pub struct RetirementService {}

#[tonic::async_trait]
impl Retirement for RetirementService {
    async fn can_retire(
        &self,
        request: Request<RetirementRequest>,
    ) -> Result<Response<RetirementResponse>, Status> {
        println!("Got a request {:?}", request);

        let req = request.into_inner();

        let mut can = false;
        if req.age >= 65 {
            can = true;
        } else if req.service_time >= 30 {
            can = true;
        } else if req.age >= 60 && req.service_time >= 25 {
            can = true;
        }

        print!("{}", can as i32);
        let reply = RetirementResponse { can_retire: can };

        Ok(Response::new(reply))
    }
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let addr = "[::1]:50051".parse()?;
    let retirement_service = RetirementService::default();

    Server::builder()
        .add_service(RetirementServer::new(retirement_service))
        .serve(addr)
        .await?;

    Ok(())
}
