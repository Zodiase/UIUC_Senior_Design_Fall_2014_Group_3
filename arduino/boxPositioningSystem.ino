// Box Positioning System Pins
const int bpsCornerPin[4][2] = {
  {2, 3},
  {4, 5},
  {6, 7},
  {8, 9}
};
const int bpsTopLeft = 0;
const int bpsTopRight = 1;
const int bpsBottomRight = 2;
const int bpsBottomLeft = 3;
// Change this if north is different.
const int northPin = 1;
const int southPin = 0;

void bps_setup()
{
  pinMode(bpsCornerPin[0][0], INPUT);
  pinMode(bpsCornerPin[0][1], INPUT);
  pinMode(bpsCornerPin[1][0], INPUT);
  pinMode(bpsCornerPin[1][1], INPUT);
  pinMode(bpsCornerPin[2][0], INPUT);
  pinMode(bpsCornerPin[2][1], INPUT);
  pinMode(bpsCornerPin[3][0], INPUT);
  pinMode(bpsCornerPin[3][1], INPUT);
}

void bps_reportFace ()
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
