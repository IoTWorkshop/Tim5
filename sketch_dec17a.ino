#include <Adafruit_BMP280.h>

#include "DHT.h"
#include "string.h"
//Ardino i RPi
#include <Wire.h>
#include <SPI.h>
#include <Adafruit_Sensor.h>
#define SLAVE_ADDRESS 0x40

const int gasSensor = 0; 

#define DHTPIN 2 
#define DHTTYPE DHT11   
DHT dht(DHTPIN, DHTTYPE);

#define LED1 7
#define LED2 6
#define LED3 5

int sensorGas = 0;
int s = 0;
float h = 0;
float t = 0;
float f = 0;
float hif = 0;
float hic = 0;
char dest[500];
void setup() {
  Serial.begin(9600);
  Serial.println("Smart Greenhouse!");
  pinMode(A0, INPUT); //soil hydrometer
  //pinMode(A1, INPUT);
  pinMode(LED1, OUTPUT);
  pinMode(LED2, OUTPUT);
  pinMode(LED3, OUTPUT);
  dht.begin();

  //Arduino i RPi
  Wire.begin(SLAVE_ADDRESS);
 
}

void loop() {
  // Wait a few seconds between measurements.
  s = analogRead(A0);
  sensorGas = analogRead(A1);
  delay(2000);

  // Reading temperature or humidity takes about 250 milliseconds!
  // Sensor readings may also be up to 2 seconds 'old' (its a very slow sensor)
  h = dht.readHumidity();
  // Read temperature as Celsius (the default)
  t = dht.readTemperature();
  // Read temperature as Fahrenheit (isFahrenheit = true)
  f = dht.readTemperature(true);

  // Check if any reads failed and exit early (to try again).
  if (isnan(h) || isnan(t) || isnan(f)) {
    Serial.println("Failed to read from DHT sensor!");
    return;
  }

  // Compute heat index in Fahrenheit (the default)
  hif = dht.computeHeatIndex(f, h);
  // Compute heat index in Celsius (isFahreheit = false)
  hic = dht.computeHeatIndex(t, h, false);
  char src [50];
  Serial.print("Vrijednost gas senzora: ");
  strcpy(dest, "Vrijednost gas senzora: ");
  dtostrf(sensorGas, 6, 2, src);
  strcat(dest, src);
  strcat(dest, " ");
  Serial.println(sensorGas, DEC);
  strcpy(src, ". Vlaznost tla: ");
  strcat(dest, src);
   dtostrf(s, 6, 2, src);
  strcat(dest, src);
  strcat(dest, " ");
  
  Serial.print("Vlaznost tla: ");
  Serial.print(s);
  Serial.print(" - ");
  if (s>=1000) {
    Serial.println("Uredjaj nije prikljucen.");
    strcpy(src, " - Uredjaj nije prikljucen. ");
    strcat(dest, src);
    digitalWrite(LED1, LOW);
  }
  if(s < 1000 && s >= 600) {
    Serial.println("UPOZORENJE!");
    Serial.println("Zemljiste je suho.");
    strcpy(src, " - UPOZORENJE! Zemljiste je suho. ");
   strcat(dest, src);
     digitalWrite(LED1, HIGH);
    
  }
  if (s < 600 && s >= 370) {
    Serial.println("Zemljiste je vlazno");
    strcpy(src, " - Zemljiste je vlazno. ");
    strcat(dest, src);
     digitalWrite(LED1, LOW);
  }
  if (s < 370) {
    Serial.println("Senzor je u vodi.");
    strcpy(src, " - Senzor je u vodi. ");
    strcat(dest, src);
    digitalWrite(LED1, LOW);
  }
  
  Serial.print("Vlaznost zraka: ");
  strcpy(src, " Vlaznost zraka: ");
  strcat(dest, src);
   dtostrf(h, 6, 2, src);
  strcat(dest, src);
  Serial.print(h);
  Serial.print(" %\t");
  Serial.print("Temperatura: ");
  strcpy(src, " Temperatura: ");
  strcat(dest, src);
   dtostrf(t, 6, 2, src);
  strcat(dest, src);
  Serial.print(t);
  Serial.print(" *C ");
  Serial.print(f);
  Serial.print(" *F\t");
  Serial.print("Indeks toplote: ");
  Serial.print(hic);
  Serial.print(" *C ");
  Serial.print(hif);
  Serial.println(" *F");
  if (t > 20) {
    Serial.println("UPOZORENJE!");
    Serial.println("Temperatura je iznad 20*C!");
   strcpy(src, " UPOZORENJE! Temperatura je iznad 20*C! ");
     strcat(dest, src);
    digitalWrite(LED2, HIGH);
  }
  else
    digitalWrite(LED2,LOW);
  
  if (sensorGas > 100) {
    Serial.println("UPOZORENJE!");
    Serial.println("Prisustvo dima i drugih gasova, sto moze asocirati na pozar!");
    strcpy(src, " UPOZORENJE! Prisustvo dima i drugih gasova, sto moze asocirati na pozar! ");
     strcat(dest, src);
    digitalWrite(LED3, HIGH);
  }
  else
    digitalWrite(LED3, LOW);

  //Arduino i RPi
  Wire.onRequest(sendData1);
    delay(400);
  Serial.println(dest);
}


//Arduino i RPi
void sendData1(){
 
    Serial.println(dest);
   // Wire.onRequest(sendData2);
    Wire.write(dest);
//
//    Wire.write(bme.readAltitude(1013.25)); 
//    
//    Wire.write(voltage);
    
}
void reverse(char *str, int len)
{
    int i=0, j=len-1, temp;
    while (i<j)
    {
        temp = str[i];
        str[i] = str[j];
        str[j] = temp;
        i++; j--;
    }
}
int intToStr(int x, char str[], int d)
{
    int i = 0;
    while (x)
    {
        str[i++] = (x%10) + '0';
        x = x/10;
    }
 
    // If number of digits required is more, then
    // add 0s at the beginning
    while (i < d)
        str[i++] = '0';
 
    reverse(str, i);
    str[i] = '\0';
    return i;
}
void ftoa(float n, char *res, int afterpoint)
{
    // Extract integer part
    int ipart = (int)n;
 
    // Extract floating part
    float fpart = n - (float)ipart;
 
    // convert integer part to string
    int i = intToStr(ipart, res, 0);
 
    // check for display option after point
    if (afterpoint != 0)
    {
        res[i] = '.';  // add dot
 
        // Get the value of fraction part upto given no.
        // of points after dot. The third parameter is needed
        // to handle cases like 233.007
        fpart = fpart * pow(10, afterpoint);
 
        intToStr((int)fpart, res + i + 1, afterpoint);
    }
}
float getVoltage(int pin)
{
return (analogRead(pin) * 0.004882814);
}
