# IA Project 1

## ZetaAI

### Authors

**Ana Dos Santos** - a21900297 [AnSantos99](https://github.com/AnSantos99)

**André Vitorino** - a21902663 [Freeze88-2](https://github.com/Freeze88-2)

**Catarina Matias** - a21801693 [StarryNight00](https://github.com/StarryNight00)

### Tasks of each group member

**Ana Dos Santos** ()

**André Vitorino** ()

**Catarina Matias** ()


### Project's Git Repository

<https://github.com/Freeze88-2/LP2_P1.git>

### Describing Algorithmic Search

In our project, we implemented a `NegaMax` algorithm with alpha-beta cuts.
In the first phase, we implemented the NegaMax algorithm simply using
`NegativeInfinity` and `PositiveInfinity` to classify each `board`, a test
placement by the algorithm of a piece on the game board, but with no heuristic
that could help it discern a good form a bad placement. We decided to start with
this small step to make sure the loops were running correctly and to get better
acquainted with the project and the class discussed materials. When all loops
were running as intended, we implemented the primary heuristic, as described in
the next chapter, “Heuristic Approach”. Then we added alpha-beta cuts to the
`NegaMax` algorithm, both as an improvement of the code and as an optimization.
Finally, the heuristic was perfected up until the conclusion of the project.

The first section of the code overrides the `Think` method, which is mandatory
for the algorithm to work as an Artificial Intelligence and it’s invoked by the
Game Engine. Here we set the `maxDepth` to one and look at the first
`depth`-level [a `depth`-level is a sort of “inspection level”;
while it runs, the algorithm will make several test placements, the first level
being for its own next move, on the second depth the algorithm will be looking
into possible piece placements the enemy will make, assuming that they follow
the same heuristic, on the third depth it will evaluate its own options again;
this happens so on until the algorithm decides the best available move for the
current round or runs out of time]. Then, the `NegaMax` method is called to do
the aforementioned depth logic described for the current depth. The next section
starts by stopping the `timer` to set a `while-loop`. The `while-loop` will look
into how much time passed during that first part and adjusts how much time the
algorithm has to run any more depths. As long as there is time, the `loop` will
keep on evaluating more boards at deeper depths, incrementing the `maxDepth`
each time [it increments the `maxDepth` and not the `currentDepth` because of
the `NegaMax` logic; one of its loops checks if the `depth == maxDepth` to
return a `move` and a `value` and allow the loop to end]. This section ends by
returning a `move` [a piece’s shape and color, and the desired move’s column]
once the time ends or the algorithm finds a `winScore` position, one that
guarantees it will win the game. the time stops again, the ´move´ is returned
to the method, and it’s the other player’s turn.

The second section of code has the `NegaMax` algorithm logic. `NegaMax`
algorithm starts by 

### Heuristic Approach

### Flow Chart
Only if we want to explain our solution in a graphic way

![]()


## References

The following references where used during this project.

**[1]** Class Power-points

**[2]** http://blog.gamesolver.org/

**For Diagrams and FlowCharts:**

**[1]** The following site was used for both flowchart and UML class diagram.
<https://www.draw.io/>
