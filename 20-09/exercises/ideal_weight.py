def define_ideal_weight(height, gender):

    # Calculando peso ideal
    if gender.upper() == "M":
        return (72.7 * height) - 58
    elif gender.upper() == "F":
        return (62.1 * height) - 44.7
    else:
        return -1