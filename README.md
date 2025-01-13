# Izzet's (Polished) Map Generator

### This is a map generator you can use to make your own images :)
<br /><br />

## Perlin Noise Algorithm:

Create a random seed.
```
Vector2 rand = new Vector2(UnityEngine.Random.Range(1,10000), UnityEngine.Random.Range(1, 10000));
```
<br /><br />

Make a new texture with the inputted size (Size Field).
```
Texture2D texture = new Texture2D(width, height);
```
<br /><br />

Loop through x and y of the texture.
```
for (int x = 0; x < width; x++)

for (int y = 0; y < width; y++) 
```
<br /><br />

Use GetPerlinID to find the ID of that specific pixel of the seed we are using.
```
int id = GetPerlinID(new Vector2(y, x), rand); //(Across x, Up y)
```
<br /><br />

Inside GetPerlinID, we magnify the ID based on the user’s input (Input Field), this makes the map more zoomed in or more zoomed out.
```
int magnification = input * (height / 10);

float perlin = Mathf.PerlinNoise(
 (v.x + rand.x) / magnification,
 (v.y + rand.y) / magnification
);
```
<br /><br />

We then scale the ID, this allows us to use different colours depending on how big the variable is.
```
float scaledPerlin = perlin * perlinScale;
```
<br /><br />

We then round to int because we need this as a whole number and return.
```
return Mathf.RoundToInt(scaledPerlin);
```
<br /><br />

Coming back to the main function, we check if it is 0, meaning it can just be the background, no colour change needed.
```
if (id == 0) 
{
   texture.SetPixel(y, x, backgroundPerlin);
}
```
<br /><br />

If not, we need to scale the hue to get the height map.
```
else 
{
   //Base Colour
   Color scaledColour = backgroundPerlin;

   //Get HSV
   float h;
   float s;
   float v;
   Color.RGBToHSV(scaledColour, out h, out s, out v);

   //Scale to 360
   h *= 360;

   //Get new hue using id
   h = h - (scaleHueBy * id);

   //Just in case, this shouldn't happen
   h = Mathf.Clamp(h, 0, 360);

   //Back to normalised
   h /= 360;

   //Convert back
   scaledColour = Color.HSVToRGB(h, s, v);

   //Set
   texture.SetPixel(y, x, scaledColour);
}
```
To get the hue, we need to get our starting colour, and then get the Hue from the ‘RGBToHSV’ function in the ‘Color’ class. 
<br /><br />
We then scale it by 360, it is currently a decimal and this will get it to the range we are familiar with.
<br /><br />
Then, we reduce hue by the ‘(scaleHueBy * id)’, scaleHueBy is a global parameter which you can be edited in the inspector.
<br /><br />
Lastly we set that specific pixel.
<br /><br />

Lastly, we change the filter, apply, and display to the user.
```
texture.filterMode = FilterMode.Point;
texture.Apply();
map.texture = texture;
```
<br /><br />

Here are some output examples:
![alt text](https://github.com/JamesNeville1/Izzets-Maps/blob/main/Izzet's%20Maps/MapOutput%20(2).png)
![alt text](https://github.com/JamesNeville1/Izzets-Maps/blob/main/Izzet's%20Maps/MapOutput%20(4).png)

## Random Walk Algorithm:

Firstly, we get the middle of the image and set the size of the image (Size Field).
```
Vector2Int currentpos = ReturnMid();
Texture2D texture = new Texture2D(width, height);
```
<br /><br />

Getting the middle is fairly simple, we just divide the height and width by two (-1 because of array) and force it to be an int in case of decimals. We then return.
```
private Vector2Int ReturnMid() 
{
   Vector2Int v = new Vector2Int((width - 1) / 2, (height - 1) / 2);
   return v;
}
```
<br /><br />

We then make the entire background our background colour, as this algorithm likely won’t go through every pixel.
```
Color[] pixels = Enumerable.Repeat(backgroundRandomWalk, width * height).ToArray();
texture.SetPixels(pixels);
```
<br /><br />

We then iterate based on the user’s input (Input Field).
```
for (int i = 0; i < input; i++)
```
<br /><br />

We get a random direction from the 4 directions.
```
   Vector2Int dir = ReturnRandomDir();
```
```
private Vector2Int ReturnRandomDir()
{
   int i = UnityEngine.Random.Range(1, 5);
   switch (i)
   {
      case 1: return Vector2Int.left;
      case 2: return Vector2Int.right;
      case 3: return Vector2Int.up;
      case 4: return Vector2Int.down;
   }

   //This should not happen
   return Vector2Int.left;
}
```
<br /><br />

Then, we check that we are not at the bounds of the image.
```
bool atBounds = currentpos.x > width - 2 || currentpos.x < 1 || currentpos.y > height - 2 || currentpos.y < 1;
```
<br /><br />

If we are, we go back to the centre.
```
if (atBounds)
{
   currentpos = ReturnMid();
}
```
<br /><br />

We then set the pixel.
```
texture.SetPixel(currentpos.y, currentpos.x, tilesRandomWalk);
```
<br /><br />

At the end of the process, we filter, apply, and set the texture like last time.
```
texture.filterMode = FilterMode.Point;
texture.Apply();
map.texture = texture;
```
<br /><br />

This creates a very organic cave system from a top down perspective:
![alt text](https://github.com/JamesNeville1/Izzets-Maps/blob/main/Izzet's%20Maps/MapOutput%20(1).png)
