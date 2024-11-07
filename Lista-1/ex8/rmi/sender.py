import Pyro5.api

def main():

    uri = "PYRONAME:credit_calculator"
    bank_credit_service = Pyro5.api.Proxy(uri)

    average_balance_input = input("Enter your average balance: ").strip()
    if not average_balance_input:
        print("Average balance cannot be empty.")
        exit()
    
    try:
        average_balance = float(average_balance_input)
        if average_balance < 0.0:
            print("Invalid balance. Enter a valid positive number greater than or equal to zero.")
            exit()
    except ValueError:
        print("Invalid average balance. Please enter a numeric value.")
        exit()

    try:
        credit = bank_credit_service.credit_calculator(average_balance)
        print(f"Credit: {credit:.2f}".format())
    except Exception as e:
        print(f"Failed to connect to the Pyro5 server: {e}")

if __name__ == "__main__":
    main()
