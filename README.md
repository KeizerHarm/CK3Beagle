# CK3 Beagle

This is a [static analyser](https://en.wikipedia.org/wiki/List_of_tools_for_static_code_analysis), specifically a [smell detector](https://en.wikipedia.org/wiki/Code_smell), for mods for [Crusader Kings III](https://ck3.paradoxwikis.com/Crusader_Kings_III_Wiki), implemented as a [Visual Studio Code](https://code.visualstudio.com/) extension.

## Mods?

[Mods](https://en.wikipedia.org/wiki/Video_game_modding) are game modifications developed by hobbyist community members to enhance and expand their game experience, and they are downloaded and installed by thousands of users. They range from small fixes to total overhauls, and some are popular enough that a significant fraction of the game playerbase cannot go without them.

Mods are written in the Paradox Scripting Language (dubbed PSL by me as it has no official name), specifically in the CK3 dialect, which is a mixed-paradigm [declarative](https://en.wikipedia.org/wiki/Declarative_programming)/[imperative](https://en.wikipedia.org/wiki/Imperative_programming) language that started as a simple configuration script but accumulated more and more powerful, sometimes idiosyncratic features.

## Smells?

PSL is a large language, and between its many features and specifiable entities are many rule inconsistencies and eccentricities of behaviour. The largest mods are huge and even the devoted work of the teams maintaining them can sometimes leave areas of script less well than would be ideal. Often, every feature in a good mod will *work*, it will have been tested, but parsing and maintaining the script will take more time than it needs.

Smells are examples of anti-patterns in script; they are ways to program in an unwieldy, less maintainable way. A good example is Overcomplicated Trigger; see [here](https://github.com/KeizerHarm/CK3Beagle/blob/main/Documentation/Smells/OvercomplicatedTrigger.md) for its variations.

## How does the tool work?

...

## Rules!

Smells are often subjective and so this tool is _highly_ configurable. Every individual smell check can be enabled or disabled, and almost all have configurable threshold values. Just how many lines of script do you need to write before that scripted effect should be split into more logical parts? You decide!

To help come up with the rules appropriate for your situation, here's a handy Rule Preset finder. Simply drop the preset into your VSC configuration, fire the tool and see what it has to report.
