**************************************
*             TRAILS FX              *
* Created by Ramiro Oliva (Kronnect) * 
*            README FILE             *
**************************************


Quick help: how to use this asset?
----------------------------------

Trails FX is an asset for drawing fast and smooth trails behind moving objects.
Just add "Trail Effect" script to any gameobject and configure its properties.

Note: in URP, Space Distortion effect requires Opaque Texture enabled in Universal Rendering Pipeline asset.


Scripting Support
-----------------

Trails FX is designed to be easy to use with the inspector.
If you need to add or change certain values at runtime you can use this code:

TrailsFX.TrailEffect trail = myGameObject.GetComponent<TrailsFX.TrailEffect>();
trail.Clear() : clears any existing trail
trail.xxxx = value; (you can change any property shown in the inspector). Check out the TrailEffect code for a list of properties.
trail.UpdateMaterialProperties() : refresh material settings (call this after changing any property using code)


Hints
-----

If target is moved using physics methods (eg. AddForce) and trails are not smooth enough, enable Rigidbody interpolation setting.
Also make sure physics methods are used in FixedUpdate().



Support & Feedback
------------------

Every property in the inspector shows a tooltip with some info when you pass the mouse over them.
If you have any issue or question please use the contact info below.
Also if you like Trails FX, please rate it on the Asset Store. It encourages us to keep improving it! Thanks!

Contact details:
* Email: contact@kronnect.com
* Support Forum: http://kronnect.com/support
* Twitter: @KronnectGames



Future updates
--------------

All our assets follow an incremental development process by which a few beta releases are published on our support forum (kronnect.com).
We encourage you to signup and engage our forum. The forum is the primary support and feature discussions medium.

Of course, all updates of Trails FX be eventually available on the Asset Store.



More Cool Assets!
-----------------
Check out our other assets here:
https://assetstore.unity.com/publishers/15018



Version history
---------------

Version 1.6.2
- [Fix] Fixed black trail artifact issue with some animated skinned renderers

Version 1.6.1
- [Fix] Fixed memory leak issue when baking skinned meshes

Version 1.6
- Added "Additive Tint Color" option to Space Distortion style

Version 1.5.92
- Startup optimizations

Version 1.5.91
- Animation states now are recognized regardless of layer

Version 1.5.9
- [Fix] Fixed skinned mesh scaling issue

Version 1.5.8
- [Fix] Fixed animation states only option when target is not a character

Version 1.5.6
- [Fix] Fixed world position relative change algorithm

Version 1.5.5
- Improved interpolated trail
- Added mesh pool size configurable option
- Memory optimization when enabling "Use Last Animation" option

Version 1.5.4
- [Fix] Fixed trail issue for very small durations during start

Version 1.5.3
- [Fix] Fixed trail sequence when active property is toggled on/off
- [Fix] Fixed scale over time issue on rotated objects

Version 1.5.2
- [Fix] Fixed material leak

Version 1.5.1
- [Fix] Fixed Space Distortion rendering issue in Unity 2019 for builtin

Version 1.5
- Added "SubMesh Mask" option to filter submeshes

Version 1.4.2
- [Fix] Fixed wrong scale of skinned mesh trails when parent scale is different than skinned mesh's gameobject

Version 1.4.1
- [Fix] Trails are now rendered to the same layer than gameobject

Version 1.4
- Improved performance of color/scale and other gradient-type fields computation
- Added "Steps Buffer Size" to inspector which allows you to increase or decrease the number of active trail steps

Version 1.3
- Added "Cull Mode" option to inspector

Version 1.2
- Added 'Execute in Edit Mode' option to inspector

Version 1.1
- Added World Position Relative To option

Version 1.0.1
- [Fix] Fixed trail size when character is scaled

Version 1.0
- Initial version
