#include <Adafruit_NeoPixel.h>

const int numberOfBoards = 1;
int boardToPinMap[] = {6};
Adafruit_NeoPixel strip[numberOfBoards];

const int numSolidLights = 2 * 8;
const int numDimmableLights = 30 / 3;

byte lightRed[numSolidLights + numDimmableLights];
byte lightGreen[numSolidLights + numDimmableLights];
byte lightBlue[numSolidLights + numDimmableLights];
short lightStart[numSolidLights + numDimmableLights];
short lightEnd[numSolidLights + numDimmableLights];

byte numLightDefs = 0;
short numPixelDefs = 0;
const int numAllowedPixelDefinitions = 1*64;
byte pixelPos[numAllowedPixelDefinitions];
byte pixelBoard[numAllowedPixelDefinitions];

int value = 0;
int lastvalue = 0;



void setup() {
  Serial.begin(115200);
  for (int board = 0; board < numberOfBoards; board++)
  {
    strip[board] = Adafruit_NeoPixel(64, boardToPinMap[board], NEO_GRB + NEO_KHZ800);
    strip[board].begin();
  }
}

int loopCount = 0;
int brightness = 16;
void loop() { 
  strip[0].setPixelColor(0, strip[0].Color(50,0,0));
  strip[0].show();
  // Wait for the header to come in.
  while (Serial.available() < 2) { delay(1); }
  
  // Read the header information.
  byte header = Serial.read();
  byte numberOfBytes = Serial.read();

  for (int i = 0; i < 64; i++)
  {
    strip[0].setPixelColor(i, strip[0].Color(0,0,0));
  }


  strip[0].setPixelColor(2 + numberOfBytes, strip[0].Color(50,0,0));
  strip[0].show();

  // Wait for the body to come in.
  while(Serial.available() < numberOfBytes)
  { 
    for (int i = 0; i <= numberOfBytes; i++)
    {
      if (Serial.available() < i)
      {
        strip[0].setPixelColor(1 + i, strip[0].Color(0,0,0));      
      }
      else
      {
        strip[0].setPixelColor(2 + i, strip[0].Color(0,50,0));        
      }
    }
    strip[0].show();
    delay(2);
  }

  // Pick what to do based on the header code.
  if (header == 255)
  {
    initMode(numberOfBytes);
    return;
  }
  else if (header == 254)
  {
    numPixelDefs = 0;
    numLightDefs = 0;
    initMode(numberOfBytes);
    return;
  }
  else if (header == 128)
  {
    displayFrame(numberOfBytes);
  }

  // Refresh panels
  for (int i = 0; i < numberOfBoards; i++)
  {
    strip[i].show();
  }
  
  loopCount++;
  while(Serial.read() != -1){}
}

void initMode(int numberOfBytes)
{
  int numberOfPixels = (numberOfBytes - 3) / 2;

  if (numberOfPixels < 1) return;

  lightRed[numLightDefs] = Serial.read();
  lightGreen[numLightDefs] = Serial.read();
  lightBlue[numLightDefs] = Serial.read();
  lightStart[numLightDefs] = numPixelDefs;
  
  for (int pixel = 0; pixel < numberOfPixels; pixel++)
  {
    pixelPos[numPixelDefs] = Serial.read();
    pixelBoard[numPixelDefs] = Serial.read();
    numPixelDefs++;
  }
  
  lightEnd[numLightDefs] = numPixelDefs;

  numLightDefs++;
}

void displayFrame(int numberOfBytes)
{
  for (int i = 0; i < numberOfBytes; i+=2)
  {
    int lightDef = Serial.read();
    int lightValue = Serial.read();
    setLight(lightDef, lightValue);
  }
}

// Sets the given light definition to the given intensity.
// light: Which light definition you want to turn on or off.
// intensity:
// For solid lights: [0 or 1]
// For dimable lights: [0, 1, 2, ... , 254, 255] 
void setLight(int light, byte intensity)
{
  // Go through every pixel in this light definition.
  for (int i = lightStart[light]; i < lightEnd[light]; i++)
  {
    // Get all the information about how the pixel should look.
    byte board = pixelBoard[i];
    byte x = (unsigned byte)(pixelPos[i] >> 4);
    byte y = pixelPos[i] & 0x0F;
    byte r = lightRed[light] * intensity;
    byte g = lightGreen[light] * intensity;
    byte b = lightBlue[light] * intensity;

    if (x < 0 || x > 7) x = 0;
    if (y < 0 || y > 7) y = 0;

    // Set the pixel.
    strip[board].setPixelColor((y * 8) + x, strip[board].Color(r, g, b));
  }
}

