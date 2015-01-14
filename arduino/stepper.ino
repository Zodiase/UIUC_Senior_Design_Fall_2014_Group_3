#include <math.h>
#include <Stepper.h>

// Stepper Pins
const int stepperPin1 = 10;
const int stepperPin2 = 11;
const int endStopPin = 12;

// Stepper Step Size
const float stepSizeDegrees = 1.8;

// Stepper Steps in Total (This should conform to the step size)
const int stepsPerRevolution = 200;
// Stepper Speed (Rounds per Minute)
const int stepperSpeedRPM = 30;

Stepper stepper (stepsPerRevolution, stepperPin1, stepperPin2);

int stepperAngleInSteps = 0;

void motor_setup()
{
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
  stepper.step(targetAngleInSteps - stepperAngleInSteps);
  
  // Assume the previous operations are successful.
  stepperAngleInSteps = targetAngleInSteps;

  // Report ending angle.
  motor_reportAngle();
}

void motor_reset()
{
  while (digitalRead(endStopPin) != HIGH) {
    stepper.step(-1);
  }
  stepperAngleInSteps = 0;
}

void motor_reportAngle()
{
  float realAngle = stepperAngleInSteps * stepSizeDegrees;
  Serial.print(realAngle);
  Serial.print("\n");
}
