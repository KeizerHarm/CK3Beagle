# Large Unit

If something gets too big it becomes harder to grasp what it does, and harder to change it when necessary. When a single unit of script approaches the size of a chapter from a novel, it is time to split it up into pieces, each named after what it does, so that the unit that was formerly bloated can now be read easily, and the parts can be understood by themselves.

Of course the CK3 script is quite verbose to begin with, and many things will need a lot of boilerplate script just to exist. Not everything can be cloven. So we're going to be considering the size of only certain kinds of units. Consider first the scripted modifiers, triggers, effects, script values (together called "macros"). These can be fully decomposed into more macros, cut down to the desired size.

For the other structures, like events and decisions; when they grow too bloated we need to consider the parts of them that can be extracted to a macro. That is, for instance, trigger script from its `trigger` block, effect script from its `immediate` and `option` blocks, modifiers from its `weight_multiplier`. An event can so be trimmed down, and its constituent parts be read and understood in isolation.

## Configuration
There are three cases for this smell, explained below. For each, besides Severity, the threshold value can be configured: what is the minimum number of lines (for files) or statements (for blocks) that counts as 'too large'?

## LU.1: Large File
Brevity is the soul of wit, and when files get too long they become harder and harder to navigate. Splitting it up into parts also allows for submods to override one part but not the other. Where feasible, try to stay below a certain total line count.

## LU.2: Large Macro
Macros (scripted triggers, scripted effects, script values and scripted modifiers) should have a single understandable purpose that they can be named after. If a single macro is too long, it may be doing too much, and you start needing to read all of these lines in order to understand when to use it.

## LU.3: Large Non-Macro Block
This describes when a part of a larger declaration, containing trigger or effect script, exceeds the threshold size. The block or parts of it can be extracted to scripted triggers or effects respectively. In the case of events, those can be written above the event itself.

<table>
  <thead>
    <tr>
      <th>Original</th>
      <th>Refactored</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><pre>
overlong_event.1 = {
	#[...]
	trigger = {
		#23 lines of triggers
		any_courtier = {
			#37 lines of triggers
		}
	}
	#[...]
}
</pre></td>
      <td><pre>
scripted_trigger overlong_event_1_ruler_suitable = {
	#23 lines of triggers
}
scripted_trigger overlong_event_1_courtier_suitable = {
	#37 lines of triggers
}
overlong_event.1 = {
	#[...]
	trigger = {
		overlong_event_1_ruler_suitable = yes
		any_courtier = {
			overlong_event_1_courtier_suitable = yes
		}
	}
	#[...]
}
</pre></td>
    </tr>
  </tbody>
</table>