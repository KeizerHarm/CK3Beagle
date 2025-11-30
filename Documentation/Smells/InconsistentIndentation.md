# Inconsistent Indentation

Well-formed indentation is not just a matter of pleasing the eye. In a language like the CK3 script, where many-levelled scoping can take your script all the way to the right of the screen, keeping an accurate count of your current 'depth' is key to putting the right amount of close-brackets at the end, and preventing errors from arising. After {, every next line should be moved to the right, except for its closing } which should move that same distance to the left again.

## Configuration
Aside from severity for each of the two cases, there is configuration for:

### Allowed Indentation Types
How can your file be indented? Tabs are the most standard in CK3 vanilla at time of writing, and four-spaces also commonly used. You can allow any number of indentation types: Tabs, Four Spaces, Three Spaces, Two Spaces.

### Comment Handling
There's three possible ways to handle lines that are entirely comments:
#### Commented Brackets Count
A comment line that contains an opening bracket will indent its contents to the right, and one with a closing bracket will move what follows to the left. Essentially a comment is treated as a regular line.
<pre>
good_catholic_hof_trigger = {
	has_primary_title = title:k_papacy
	#Let's give this extra attention {
		num_sinful_traits = 0
	# } End of special case
}
</pre>

#### No Special Treatment
A comment line needs to be correctly indented itself, but it does not move the indentation for the next lines no matter what it contains. A comment line that contains an opening bracket will indent its contents to the right, and one with a closing bracket will move what follows to the left. Essentially a comment is treated as a regular line.
<pre>
OR = {
	# Or should we use AND = {?
	gold >= 25
	piety >= 25
}
</pre>

#### Comments Ignored
A comment line is completely disregarded for indentation checking. That is, it does not need to be properly indented _itself_.
<pre>
vanilla_trigger = {
#   gold >= 25
	piety >= 25
}
</pre>

## II.1: Unexpected Type
The whole file is indented fine... but in a different way than expected. For example, instead of by Tabs, or by quartets of spaces, it is indented with triplets of spaces. This is determined by the rule [Allowed Indentation Types](https://github.com/KeizerHarm/CK3Beagle/blob/main/Documentation/Smells/InconsistentIndentation.md#allowed-indentation-types). Besides Tabs, Two Spaces, Three Spaces and Four Spaces, the detected indentation type could also be Inconclusive. Meaning the file has a jumble of different indentations and the tool cannot tell what it's meant to be.

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
maternal_grandparent_is_zealous_trigger = {
  mother ?= {
    any_parent = {
      has_trait = zealous
    }
  }
}
</pre></td>
      <td><pre>
maternal_grandparent_is_zealous_trigger = {
    mother ?= {
        any_parent = {
            has_trait = zealous
        }
    }    
}
</pre></td>
    </tr>
  </tbody>
</table>

## II.2: Inconsistency
The indentation of the file itself is coherent, except for this line or these lines. They should be corrected by being moved the right amount to the left or right, using the proper spacing characters.

It's worth nothing that this smell also triggers for indentation with mixed usage of tabs and spaces. Even if the distance looks right to your monitor, it will fail in different contexts, as tabs do not have a consistent width. Get rid of all at once by mass-replacing the spaces with tabs, or vice versa.

<table>
  <thead>
    <tr>
      <th>Original</th>
      <th>Refactored</th>
	  <th>Also valid</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><pre>
maternal_grandparent_is_zealous_trigger = {
mother ?= {
    any_parent = {
        has_trait = zealous
    }
}
}
</pre></td>
      <td><pre>
maternal_grandparent_is_zealous_trigger = {
    mother ?= {
        any_parent = {
            has_trait = zealous
        }
    }    
}
</pre></td>
      <td><pre>
maternal_grandparent_is_zealous_trigger = {
    mother ?= { any_parent = { has_trait = zealous } }    
}
</pre></td>
    </tr>
  </tbody>
</table>
