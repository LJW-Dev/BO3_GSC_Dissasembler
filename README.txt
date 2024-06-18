just a few things:
1. if you are dissasembling a large file, do not click off the window while it is loading or the program will throw an exception

2. I have tried to guess the hashed names of each gsc file include, but they may be different so if you see a getfunction or function call that uses a hash for 
the function name, try some variations of names. for example, "scene_debug_shared" doesn't have the hash of "scene_debug" but instead just "scene". The rules are loosely:
remove any _ at the front of the gsc and the _shared at the end and hash it but as i said, not all gscs follow this.