import Pyro5.api

def main():

    uri = "PYRONAME:define_ideal_weight"
    ideal_weight_service = Pyro5.api.Proxy(uri)

    height_input = input("Enter your height (in meters): ").strip()
    if not height_input:
        print("Height cannot be empty.")
        exit()
    
    # Por conta da conversão
    try:
        height = float(height_input)
        if height < 1.0:
            print("Invalid height. Enter a valid positive number greater than or equal to 1 meter.")
            exit()
    except ValueError:
        print("Invalid height. Please enter a numeric value.")
        exit()
    
    gender = input("Enter your gender (F/M): ").strip().upper()
    if not gender:
        print("Gender cannot be empty.")
        exit()
    if gender not in ["M", "F"]:
        print("Invalid gender. Use M for male or F for female.")
        exit()

    # Chamar o método remoto
    try:
        ideal_weight = ideal_weight_service.define_ideal_weight(height, gender)
        print(f"Ideal weight: {ideal_weight:.2f} kg")
    except Exception as e:
        print(f"Failed to connect to the Pyro5 server: {e}")

if __name__ == "__main__":
    main()
