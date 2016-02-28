#Unity Notes
For those tricky concepts, y'know...

## Screen Resolution, from Aspect Ratio, Orthographic Camera Size, and Pixels Per Unit

Pixels Per Unit maps the number of pixels in your sprite to that number of Unity units, and can be set on a per-sprite basis. The default value is 100. Avoid setting this too low as the physics engine deals in units and doesn't like handling numbers that are too large. Keeping the default is usually ideal(?)

Orthographic Camera Size, which can be specified in the Inspector panel when you select a camera object, is the half the height in Unity units. The default value is 5, meaning the top and bottom are 5 units up and down from the origin, giving a total height of 10. Combining this with the default PPU, we get a default height of 10x100 = 1000px.

The width is then determined by the aspect ratio. For example with a 2:3 mobile portrait ratio we get a default width of 1000/3x2 = 666px

To get the game scene to display nicely across multiple devices with different resolutions and aspect ratios, start with a standard (base) aspect ratio and resolution, and scale based on that and the actual aspect ratio and resolution. To maintain the same aspect ratio of the game scene across devices of different aspect ratios, scale the Camera viewport using `Camera.rect` (which is in normalized units) to introduce either a letterbox or pillarbox instead of warping the scene. See [here](http://answers.unity3d.com/questions/618058/mobile-device-screen-sizes.html) for an example. For maintaining pixel perfect graphics, see [here](http://blogs.unity3d.com/2015/06/19/pixel-perfect-2d/).

## Figuring out sprite sizes
See [here](https://www.captechconsulting.com/blogs/understanding-density-independence-in-android) for a how-to on dealing with Android screen sizes.

In general start large and scale down. See [here](http://developer.android.com/guide/practices/screens_support.html#testing) for a table of common Android resolutions and densities.

Eg. For the mouse, say I want its width to almost fill the screen width. Let's look at a 2560x1600px xhdpi(320dpi) device, which jas a physical size of 8x5in. Say I want my mouse to be 4in, which will be 1280px at this density. So I should start with a 1280px sprite, and scale down accordingly.(Have to take into account the default aspect ratio I'm choosing as well though).

To get the screen resolution and density, get the fields from the Screen class: `Screen.currentResolution` and `Screen.dpi` (note that from my testing with Unity Remote, it doesn't seem to be picking up the mobile device's Screen but the computer's screen instead EDIT: somehow `Screen.width` and `Screen.height` seem to work?). Once these values are known, one can scale from the base dpi and resolution accordingly (320dpi and 1280px in my case).
