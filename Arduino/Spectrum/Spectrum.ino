#include <Adafruit_NeoPixel.h>
#ifdef __AVR__
  #include <avr/power.h>
#endif
#define PIN 6

Adafruit_NeoPixel strip = Adafruit_NeoPixel(64, PIN, NEO_GRB + NEO_KHZ800);

int counter = 0;
int value = 0;
int lastvalue = 0;


void setup() {
  Serial.begin(115200);
  strip.begin();
  strip.show(); // Initialize all pixels to 'off'
}

int loopCount = 0;
int brightness = 16;
bool flash = false;
void loop()
{
  if (Serial.available() >= 15)
  {
    value = Serial.read();
    if (value > 252)
    {
      flash = true;
    }
    if (counter % 2 == 0)
    {
      int row = counter / 2;
      setRow(strip.Color((brightness/16)*counter, 0, (brightness/16)*(16 - counter)), row, 8, value);
    }
    counter++;
    
    if (counter > 15) 
    {
      counter = 0;
      if (flash)
      {
        setColor(strip.Color(255,255,0));
        flash = false;
      }
      strip.show();
    }
   }
   loopCount++;
}

void setRow(uint32_t c, int row, int rowHeight, byte value)
{
    int pixelHeight = map(value, 0, 255, 0, rowHeight);

    int count = 0;
    for(int i = 7 - row; i < 64; i += 8) {
      if (count < pixelHeight)
      {
        strip.setPixelColor(i, c);
      }
      else if(count == pixelHeight)
      {
        strip.setPixelColor(i, strip.Color(25, 25, 25));
      }
      else
      {
        strip.setPixelColor(i, strip.Color(0, 8, 0));
      }
      count++;
    }
    //delay(1);
}

void setColor(uint32_t c)
{
    for(int i=0; i< strip.numPixels(); i++) {
      strip.setPixelColor(i, c);
    }
}
