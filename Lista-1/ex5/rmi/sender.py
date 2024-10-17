import Pyro5.api

def main():

    uri = "PYRONAME:define_category"
    swimmer_category_service = Pyro5.api.Proxy(uri)

    age_input = input("Enter your age: ").strip()
    if not age_input:
        print("Age cannot be empty.")
        exit()
    
    try:
        age = int(age_input)
        if age < 5:
            print("Invalid age. Enter a valid positive number greater than or equal to 5 years old.")
            exit()
    except ValueError:
        print("Invalid age. Please enter a numeric value.")
        exit()

    try:
        category = swimmer_category_service.define_category(age)
        print(f"Category: {category}")
    except Exception as e:
        print(f"Failed to connect to the Pyro5 server: {e}")

if __name__ == "__main__":
    main()
