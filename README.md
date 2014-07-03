IOB2-format-evaluate
====================

# Input_data format #

C1[tab]C2[tab]...[tab]Answer[tab]Predict

for example:

HIV-1	NN	B-gene	B-gene
enhancer	NN	I-gene	I-gene
following	VBG	O O
T-cell	NN	O B-gene
stimulation	NN	O B-location

# Output #

precision, recall, f-score, accuracy


#Command

>IOB2-format-evaluate.exe [intputFile] [options...]

option:
  type_names for harmonic mean
 
 example:
>IOB2-format-evaluate.exe input.txt gene location

