# MinecraftAutoFisher
MinecraftAutoFisher is a simple application designed to help automate the fishing process in Minecraft. This works in the latest versions of Minecraft and is designed specifically to get around the recent changes regarding treasure enchantments.

## Why?
Fishing is a rather tedious (and boring) process. However, it's also a great way to get:
- Food (Cod, Salmon, ~~Pufferfish~~).
- Enchanted Bows.
- Enchanted Fishing Rods.
- Enchanted Books.
- Saddles and Name Tags. 
- Nautilus Shells.
- Experience (Directly by fishing or by using a grindstone on the enchanted gear). 

In particular, it's not hard to get nearly or even fully maxed out gear by just fishing. 

## How It Works
The idea is relatively simple, albeit inefficient. 

### Calibration 
First, the program needs to figure out where the bobber is. This is done through a process called *calibration*. Here, the program will take an initial screenshot of your Minecraft client and look for the bobber (coming out of the fishing rod). The program does this by looking for a shade of *red* in your screenshot. Once it finds the shade of red, the program assumes that this is the bobber. Once it finds the bobber, the program will make a note of the general location of the bobber. Then, when the program needs to check whether the bobber is actively waiting for something to be caught, it can simply check that location as opposed to the entire client. 

### The Loop
The program will continuously do the following.
- Take a screenshot of your Minecraft client.
- Check if the bobber is *visible* in the general location (which was defined by the calibration process).
    - If the bobber is clearly visible in the general location, then the bobber must still be actively waiting for a fish and we should not continue on. 
- At this point, the bobber should not be *visible*. In other words, the bobber should be *underwater* (where there is barely any bright red from your bobber visible).
    - Here, the program temporarily disables the loop process. In other words, the program will no longer continuously check the bobber location.
- The program will reel in your fishing rod by pressing right-click. 
- Then, the program will wait 750 milliseconds before right-clicking again to reel out the rod.
- After this happens, the program will wait an additional 2.5 seconds before enabling the loop process. 

## Some Things to Note
When using this program, there are a few things to consider.

### Regarding the Program
- This program can only be used on a Windows system. If you want to use this program on a Mac or Linux system, you will need to find the appropriate replacements for the right-click functions (See `MouseOperations.cs`). All the other code should work properly on a different system.
- Once started, you cannot use your computer for anything else. You may still run background tasks as long as it doesn't interfere with the mouse or block/un-focus the Minecraft client.
- The program will consume quite a bit of CPU (~20%). This is due to the constant image processing that is done.
- As implied, the Minecraft process must always be visible. 
- To stop the program, close the AutoFisher window.

### Regarding Minecraft
- If you plan on using this program for a very long period of time, you should build a protective box around you. Otherwise, you may die from phantoms or other hostile mobs.
    - What I do is I build a 10 (x-coordinate) x 10 (y-coordinate) x 10 (z-coordinate) glass box. In other words, you are essentially encasing yourself with glass blocks. 
    - Inside the glass box, I fill 5-6 blocks with just water *source* blocks. Do not include any other blocks in the water tank.
    - On the level where you are planning on fishing, place some hoppers as the "shore." Connect these hoppers to a bunch of chests so that you don't lose any loot. In particular, with a fully maxed out fishing rod, 12 hours of auto-fishing usually fills up 5-6 double chests.
- Keep in mind that some Minecraft servers will not allow the use of a program like this. This program can be considered an *auto-clicker* or *macro*. Thus, use at your own risk.

## Running & Compiling
The simplest way to edit and compile this program is to install an IDE. [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/) is an excellent choice. If you install this IDE, then you can simple open the `.sln` file to open the entire project.

More information will be provided later.

## License
For all code *not* under the `AutoFisher/Imaging` folder, the license is MIT.

All code that is in `AutoFisher/Imaging` was taken from the [Accord.NET](https://github.com/accord-net/framework) library and modified to suit my needs. These files are licensed under the GNU Lesser General Public License.