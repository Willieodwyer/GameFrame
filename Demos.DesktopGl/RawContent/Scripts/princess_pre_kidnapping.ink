VAR x_pos = 0
VAR y_pos = 0
VAR fwacked = false

Why hello you must be Taikatou
Thank you for coming out to meet me
I heard there was bandits and thought and escort would be the safest way into the village
Are you the escort the village sent
*   Yes
    ~ x_pos = 15
    ~ y_pos = 10
    Guard: I don't think so, I will be taking you now.
    Fwack!!!
    ~ fwacked = true
    The Princess and that fiend of her guard fled to the north.
    Darn it.
*   No
    Ok then I will wait here until the escort arrives
- ->END