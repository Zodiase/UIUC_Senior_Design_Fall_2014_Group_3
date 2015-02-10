#include <math.h>
#include <Stepper.h>

// Stepper Pins
const int stepperPin1 = 11;
const int stepperPin2 = 12;
const int endStopPin = A1;
const int resetClearPin = A2;

// Stepper Step Size
const float stepSizeDegrees = 1.8;

// Stepper Steps in Total (This should conform to the step size)
const int stepsPerRevolution = 100;
// Stepper Speed (Rounds per Minute)
int stepperSpeedRPM = 5;

Stepper stepper (stepsPerRevolution, stepperPin1, stepperPin2);

int stepperAngleInSteps = 0;
const int endStopCountThreshold = 1000;

void motor_setup()
{
  pinMode(endStopPin, INPUT);
  //digitalWrite(endStopPin, HIGH); // connect internal pull-up
  pinMode(resetClearPin, OUTPUT);
  digitalWrite(resetClearPin, LOW);
  //motor_resetEndLatch();
  // Set the speed of the stepper motor.
  stepper.setSpeed(stepperSpeedRPM);
}

void motor_turnTo(float angle)
{
  if (angle > 360) angle = 360;
  if (angle < 0) angle = 0;
  
  // Get closest step from angle.
  int targetAngleInSteps = (int) round(angle / stepSizeDegrees);
  
  // Ask the stepper motor to turn the offset.
  stepper.step(stepperAngleInSteps - targetAngleInSteps);
  
  // Assume the previous operations are successful.
  stepperAngleInSteps = targetAngleInSteps;

  // Report ending angle.
  motor_reportAngle();
}

void motor_setSpeed(int rpm)
{
  if (rpm > 0) {
    stepperSpeedRPM = rpm;
    stepper.setSpeed(rpm);
  }
  Serial.print(stepperSpeedRPM);
  Serial.print("\n");
}

void motor_reset()
{
  motor_resetEndLatch();
  
  int stopVal;
  do {
    stopVal = digitalRead(endStopPin);
    stepper.step(1);
  } while (stopVal == LOW);
  
  motor_resetEndLatch();
  stepperAngleInSteps = 0;
  // Report ending angle.
  motor_reportAngle();
}

void motor_resetTest()
{
  Serial.print("Resetting...");
  motor_resetEndLatch();
  
  int stopVal;
  float stopAnalogVal;
  do {
    stopVal = digitalRead(endStopPin);
    stopAnalogVal = analogRead(endStopPin);
    Serial.println(stopAnalogVal);
  } while (stopVal == LOW);
  
  motor_resetEndLatch();
  Serial.print("Done\n");
}

void motor_resetEndLatch()
{
  digitalWrite(resetClearPin, HIGH);
  delay(300);
  //! Add some delay?
  digitalWrite(resetClearPin, LOW);
}

void motor_reportAngle()
{
  float realAngle = stepperAngleInSteps * stepSizeDegrees;
  Serial.print(realAngle);
  Serial.print("\n");
}

void motor_stepLeft()
{
  stepper.step(-1);
  stepperAngleInSteps += 1;
  motor_reportAngle();
}

void motor_stepRight()
{
  stepper.step(1);
  stepperAngleInSteps -= 1;
  motor_reportAngle();
}
