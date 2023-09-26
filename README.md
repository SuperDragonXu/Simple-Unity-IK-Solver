# Simple-Unity-IK-Solver
A simple unity IK solution demo. Including Fabric IK and CCD IK algorithm. This repo will keep being updated.

## Way to Use It
Drag the FabricIK_Solver or CCDIK_Solver script on your character, then specify the bones that will be effected by the IK.
There are to ways to specify the bones. 
### Method1
create a element in "Bones" list in the script, drag the end bone(could be a hand, or a foot) to it. specify the count of nodes affected by IK by filling in "Bone Count". The script will automatically retrace all the bones and add them into the Bones list in runtime.
### Method2
Leave the boneCount as zero, add the bone that will be affected on by one into the "Bones" list. The boneCount will be calculated automatically when you run the game.

Here are the explanations of the parameters in the script:

Weight: how much the IK algorithm affect the bones. Range between 0 to 1.

Lazy Bones: When set to true, the end bones will be prioritized to be affected by the IK algorithm. For example, when you apply IK to an arm of a human character and set LazyBones true, the hand will be affected first so the spine and hips will keep as still as possible.

Iterations: Iterations in IK loop.

Bones: all the bones affected by IK.

BoneCount: How many bones are there.

BoneLength: the length of each bone. No need to be set manually.

Target: the target object.

Rotate with Target: will the end bone have same rotation with the target object?

Pole: the pole that the bone nodes will bend to.
