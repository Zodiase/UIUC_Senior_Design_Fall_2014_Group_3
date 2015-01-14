// Projector Pins
const int lightPin = 13;

void projector_setup()
{
  // Configure the output pin for controlling the LED.
  pinMode(lightPin, OUTPUT);
}

void projector_setState(int isOn)
{
  if (isOn > 0) {
    digitalWrite(lightPin, HIGH);
    Serial.print("1");
  } else {
    digitalWrite(lightPin, LOW);
    Serial.print("0");
  }
  Serial.print("\n");
}
