/**
 * This is the code for the Arduino micro-controller co-operating with a PC-side program
 * to perform hardware tasks for the CT-scan demo.
 **/

/**
 * MCU side functionalities
 * + Turn LED on/off
 * + Turn servo to an angle and report when it's done (MCU inresponsive during process)
 * + Report angle value
 **/

void setup()
{
  Serial.begin(9600);
  
  motor_setup();

  projector_setup();
  
  bps_setup();

  // Print this so if the software starts before, it would be notified.
  // On the other hand, if the software starts after this point, it would
  // send a hand-shaking message and wait for this hello world message.
  helloWorld();
}

void loop()
{
  HandleCommand();
  // All serial inputs should be handled up till this point.
  Serial.flush();
}
//=============================================================================
//=============================================================================
void HandleCommand ()
{
  if (Serial.available()) {
    /**
     * Instructions
     * D{integer} : rotate to degree (closest step)
     * S{integer} : set round per minute speed
     * C : reset (move back to one end to re-calibrate)
     * A : report angle
     * L{integer} : turn LED on if > 0 and off otherwise
     * F : report which face is down and its rotation
     * Anything else: print hello world
     *
     * All instructions should have at least one hand-shaking to ensure
     * the coherence between the software and the MCU.
     **/
    char cmd1 = Serial.read();
    switch (cmd1) {
    case 'D':
      motor_turnTo(Serial.parseFloat());
      break;
    case 'S':
      motor_setSpeed(Serial.parseInt());
      break;
    case 'C':
      motor_reset();
      break;
    case 'A':
      motor_reportAngle();
      break;
    case 'L':
      projector_setState(Serial.parseInt());
      break;
    case 'F':
      bps_reportFace();
      break;
    case '\n':
      break;
    default:
      helloWorld();
      break;
    }
  }
}

// Printing Hello World
const int UnoOnboardLED = 13;
void helloWorld ()
{
  digitalWrite(UnoOnboardLED, HIGH);
  delay(5);
  digitalWrite(UnoOnboardLED, LOW);
  delay(5);
  digitalWrite(UnoOnboardLED, HIGH);
  delay(5);
  digitalWrite(UnoOnboardLED, LOW);
  Serial.print("CTSCANNER:Hello_World.\n");
}
