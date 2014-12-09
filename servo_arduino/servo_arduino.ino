#include <Servo.h>

/**
 * MCU side functionalities
 * + Turn LED on/off
 * + Turn servo to an angle and report when it's done (MCU inresponsive during process)
 * + Report angle value
 **/

Servo myservo;

const int servoPin = 10;
const int lightPin = 13;
const int potentiometerPin = A0;
// BPS == Box Positioning System
const int bpsCornerPin[4][2] = {
  {
    2, 3    }
  , {
    4, 5    }
  , {
    6, 7    }
  , {
    8, 9    }
};
const int bpsTopLeft = 0;
const int bpsTopRight = 1;
const int bpsBottomRight = 2;
const int bpsBottomLeft = 3;

// Change this if north is different.
const int northPin = 1;
const int southPin = 0;

const int initialServoAngle = 90;

unsigned char servoInputMap[360];

int servoLinked = 0;

const int servoAvgSize = 3000;

double servoPotRef = 0;
double servoPotStableThreshold = 12;//15;
unsigned long servoIdleCount = 0;
const unsigned long servoIdleThreshold = 5;

// States
enum States {
  Idle, TurningServo};

States state = Idle;
//=============================================================================
//=============================================================================
void setup()
{
  Serial.begin(9600);

  // Attach servo on start.
  attachServo();

  // Configure the output pin for controlling the LED.
  pinMode(lightPin, OUTPUT);

  // Configure the input pin for reading the potentiometer voltage.
  pinMode(potentiometerPin, INPUT);

  pinMode(bpsCornerPin[0][0], INPUT);
  pinMode(bpsCornerPin[0][1], INPUT);
  pinMode(bpsCornerPin[1][0], INPUT);
  pinMode(bpsCornerPin[1][1], INPUT);
  pinMode(bpsCornerPin[2][0], INPUT);
  pinMode(bpsCornerPin[2][1], INPUT);
  pinMode(bpsCornerPin[3][0], INPUT);
  pinMode(bpsCornerPin[3][1], INPUT);

  // Initialize servo input map.
  for (int i = 0; i < 360; i++) {
    servoInputMap[i] = 90;
  }
  servoInputMap[0] = 80;
  servoInputMap[15] = 83;
  servoInputMap[30] = 90;
  servoInputMap[45] = 94;
  servoInputMap[60] = 99;
  servoInputMap[75] = 104;
  servoInputMap[90] = 110;
  servoInputMap[105] = 115;
  servoInputMap[120] = 121;
  servoInputMap[135] = 123;
  servoInputMap[150] = 128;
  servoInputMap[165] = 134;

  // Print this so if the software starts before, it would be notified.
  // On the other hand, if the software starts after this point, it would
  // send a hand-shaking message and wait for this hello world message.
  helloWorld();
}

void loop()
{
  switch (state) {
  case Idle:
    HandleIdle();
    break;
  case TurningServo:
    HandleTurningServo();
    break;
  default:
    break;
  }
  // All serial inputs should be handled up till this point.
  Serial.flush();
}
//=============================================================================
//=============================================================================
void HandleIdle ()
{
  if (Serial.available()) {
    /**
     * Instructions
     * D{integer} : rotate to degree
     * R{integer} : feed raw data to servo
     * L{integer} : turn LED on if > 0 and off otherwise
     * P : report potentiometer reading
     * A : report angle
     * X : unbind servo
     * B : bind servo
     * F : report which face is down and its rotation
     * E : report the maximum deviation within the given number of samples
     * Anything else: print hello world
     *
     * All instructions should have at least one hand-shaking to ensure
     * the coherence between the software and the MCU.
     **/
    char cmd1 = Serial.read();
    switch (cmd1) {
    case 'D':
      // Start turning and monitoring the potentiometer feedback
      // to know when the motor stops
      state = TurningServo;
      servoPotRef = potentiometerVal(-1);

      turnTo(Serial.parseInt());
      break;
    case 'R':
      // Start turning and monitoring the potentiometer feedback
      // to know when the motor stops
      state = TurningServo;
      servoPotRef = potentiometerVal(-1);

      turnToRaw(Serial.parseInt());
      break;
    case 'L':
      // LED switching finishes in one cycle.
      if (Serial.parseInt() > 0) {
        turnLedOn();
        Serial.print("LED On.\n");
      } 
      else {
        turnLedOff();
        Serial.print("LED Off.\n");
      }
      break;
    case 'P':
      reportPotentiometerReading(Serial.parseInt());
      break;
    case 'A':
      // Angle report finishes in one cycle.
      reportServoAngle(Serial.parseInt());
      break;
    case 'X':
      detachServo();
      break;
    case 'B':
      attachServo();
      break;
    case 'F':
      reportFace();
      break;
    case 'E':
      reportError(Serial.parseInt());
      break;
    case '\n':
      break;
    default:
      helloWorld();
      break;
    }
  }
}
//=============================================================================
//=============================================================================
void HandleTurningServo ()
{
  // Set state back to idle when servo finishes turning.

  // Compare the latest potentiometer reading with the reference.
  double currPotVal = potentiometerVal(-1);
  double diff = (servoPotRef > currPotVal) ? servoPotRef - currPotVal : currPotVal - servoPotRef;
  if (diff < servoPotStableThreshold) {
    // Servo didn't move. Increment counter.
    servoIdleCount++;
  } 
  else {
    // Servo moved. Reset reference and counter.
    servoPotRef = (servoPotRef + currPotVal) / 2;
    servoIdleCount = 0;
  } // if

  if (servoIdleCount > servoIdleThreshold) {
    // Servo has been idle for enough time to be considered "stopped".
    state = Idle;
    Serial.print("Servo stopped.\n");
    Serial.print(currPotVal);
    Serial.print("\n");
  }
}

// Printing Hello World
void helloWorld ()
{
  turnLedOn();
  delay(5);
  turnLedOff();
  delay(5);
  turnLedOn();
  delay(5);
  turnLedOff();
  Serial.print("CTSCANNER:Hello_World.\n");
}

void reportPotentiometerReading (int measureCount)
{
  double potVal = potentiometerVal(measureCount);
  Serial.print(potVal, 10);
  Serial.print("\n");
}

void reportServoAngle (int measureCount)
{
  // [TODO] Convert potentiometer value to angle
  double currServoAngle = mapPotValToAngle(potentiometerVal(measureCount));
  Serial.print(currServoAngle, 10);
  Serial.print("\n");
}

// Macros for attaching and detaching servo.
void attachServo ()
{
  if (servoLinked != 1) {
    myservo.attach(servoPin);
    servoLinked = 1;
  } // if
  // else
  Serial.print("Servo attached at Pin ");
  Serial.print(servoPin);
  Serial.print(".\n");
}
void detachServo ()
{
  if (servoLinked != 0) {
    myservo.detach();
    servoLinked = 0;
  } // if
  // else
  Serial.print("Servo detached.\n");
}

void reportFace ()
{
  // faces are numbered from 1 to 6. 0 means no valid face detected.

  int cVals[4] = {
    cornerVal(0),
    cornerVal(1),
    cornerVal(2),
    cornerVal(3)
    };

  Serial.print("Scanning for face pattern.\n");

  // Scan for patterns.
  // Since the pattern can start anywhere, we need to rotate
  // the values to detect the pattern.
  int patternId = 0;
  int rotation = 0;
  
  for (int i = 0; i < 4; i++) {
    rotation = i;

    char vals[] = {
      cornerValInChar(cVals[(i + 0) % 4]),
      cornerValInChar(cVals[(i + 1) % 4]),
      cornerValInChar(cVals[(i + 2) % 4]),
      cornerValInChar(cVals[(i + 3) % 4])
      };
      if (vals[0] == '-' && vals[1] == '+' && vals[2] == '+' && vals[3] == '+') {
        patternId = 1; // Done
        break;
      } 
      else if (vals[0] == '=' && vals[1] == '=' && vals[2] == '-' && vals[3] == '=') {
        patternId = 6; // Done
        break;
      } 
      else if (vals[0] == '+' && vals[1] == '+' && vals[2] == '=' && vals[3] == '=') {
        patternId = 2; // Done
        break;
      } 
      else if (vals[0] == '=' && vals[1] == '-' && vals[2] == '+' && vals[3] == '-') {
        patternId = 3; // Done
        break;
      } 
      else if (vals[0] == '+' && vals[1] == '=' && vals[2] == '-' && vals[3] == '+') {
        patternId = 4; // Done
        break;
      } 
      else if (vals[0] == '=' && vals[1] == '+' && vals[2] == '-' && vals[3] == '=') {
        patternId = 5; // Done
        break;
      }
  }
  // Report the value in a numerical format. The integral part is the face id, the decimal part is the rotation.
  Serial.print(patternId);
  Serial.print(".");
  Serial.print(rotation);
  Serial.print("\n");
}

void reportError (int measureCount)
{
  if (measureCount <= 1) {
    Serial.print("Invalid input for measure count. Provide an integral value greater than 1.\n");
    return;
  } // if
  // else

  Serial.print("Measuring error over ");
  Serial.print(measureCount);
  Serial.print(" samples.\n");

  int minVal = -1;
  int maxVal = -1;
  for (int i = 0; i < measureCount; i++) {
    int thisVal = analogRead(potentiometerPin);
    if (minVal == -1 || minVal > thisVal) {
      minVal = thisVal;
    }
    if (maxVal == -1 || maxVal < thisVal) {
      maxVal = thisVal;
    }
  }

  Serial.print("Max: ");
  Serial.print(maxVal);
  Serial.print("; Min: ");
  Serial.print(minVal);
  Serial.print("; Deviation: ");
  Serial.print(maxVal - minVal);
  Serial.print(".\n");
}

/*====================Macros====================*/

// Macros for switching the LED on and off.
void turnLedOn () {
  digitalWrite(lightPin, HIGH);
}
void turnLedOff () {
  digitalWrite(lightPin, LOW);
}

// Macro returning the status of the corners on the box.
int cornerVal (int corner)
{
  // Check if corner is out of range.
  if (corner < 0 || corner > 3) {
    return false;
  } // if
  // else

    // Read pin values.
  int northVal = digitalRead(bpsCornerPin[corner][northPin]);
  int southVal = digitalRead(bpsCornerPin[corner][southPin]);

  bool northHigh = northVal == HIGH;
  bool southHigh = southVal == HIGH;

  if (northVal == HIGH && southVal == HIGH) {
    // Invalid state. Consider as Silent.
    return 0;
  } 
  else if (northVal == HIGH && southVal == LOW) {
    return 1;
  } 
  else if (northVal == LOW && southVal == HIGH) {
    return -1;
  } 
  else {
    // Silent state.
    return 0;
  }
}
char cornerValInChar (int cornerVal)
{
  switch (cornerVal) {
  case 1:
    return '+';
    break;
  case -1:
    return '-';
    break;
  default:
    return '=';
    break;
  }
}

/**
 * Macro for getting the potentiometer value.
 * Returns a value from 0 to 1023.
 **/
double potentiometerVal (int measureCount)
{
  if (measureCount <= 0) {
    measureCount = servoAvgSize;
  } // if

  // [TODO] Use standard deviation to improve results.
  long sum = 0;
  for (int i = 0; i < measureCount; i++) {
    sum += (long)analogRead(potentiometerPin);
  }
  return (double)sum / measureCount;
}

const double voltageToAngleMapConst[6] = {
  214.87,
  1423.3,
  -2499.8,
  1478.5,
  -327.06,
  6.1675
};
float angleVal (float voltVal)
{
  double x = 1;
  double y = 0;
  for (int i = 0; i < 6; i++) {
    y += x * voltageToAngleMapConst[i];
    x *= voltVal;
  }
  return y;
}

double mapPotValToAngle (double potVal)
{
  return angleVal(potVal * 0.0006 - 0.0051);
}

const double servoInputProjectionConst[6] = {
  143.2,
  -0.2323,
  -0.0007,
  0.000003,
  -0.000000006,
  0.000000000005
};
/**
 * Projection function returning the input to servo for the given angle.
 **/
int angleMap(int angle)
{
  /*
  int x = 1;
   double y = 0;
   for (int i = 0; i < 6; i++) {
   y += (double)x * servoInputProjectionConst[i];
   x *= angle;
   }
   return (int) ceil(y);
   */
  return (int)servoInputMap[angle];
}

/**
 * Macro for turnning the servo to an angle.
 * A projection is used to get the raw input for the given angle.
 **/
void turnTo(int angle)
{
  if (angle > 360) angle = 360;
  if (angle < 0) angle = 0;

  int realAngle = angleMap(angle);

  Serial.print("Turning to ");
  Serial.print(angle);
  Serial.print(":<");
  Serial.print(realAngle);
  Serial.print(">.\n");

  doTurn(realAngle);
}
/**
 * Macro for turnning the servo to a raw input.
 **/
void turnToRaw(int value)
{
  Serial.print("Turning to <");
  Serial.print(value);
  Serial.print(">.\n");

  doTurn(value);
}

/**
 * Restriction of the servo input.
 **/
const int MINSAFEINPUT = 5;
const int MAXSAFEINPUT = 175;
/**
 * Macro for turnning the servo while restricting the raw input.
 **/
void doTurn(int input)
{
  // Verify input value range.
  if (input < MINSAFEINPUT) {
    Serial.print("Invalid servo input. Below minimum safe value. ");
    Serial.print(input);
    Serial.print("<");
    Serial.print(MINSAFEINPUT);
    Serial.print(".\n");
  } 
  else if (input > MAXSAFEINPUT) {
    Serial.print("Invalid servo input. Over maximum safe value.");
    Serial.print(input);
    Serial.print(">");
    Serial.print(MAXSAFEINPUT);
    Serial.print(".\n");
  } 
  else {
    myservo.write(input);
    // Note that the servo takes time to reach the destination.
    delay(500);
  }
}

