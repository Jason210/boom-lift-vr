# MEWP VR Orientation

A short VR orientation and readiness experience for new and occasional users of
boom-type mobile elevating work platforms (MEWPs). Users operate a boom lift in a
simplified worksite, move the basket to an elevated target, encounter hazard zones,
and reflect on whether they feel ready to progress toward real training.

Built as a final project for BSc Computer Science (University of London),
module CM3070, project template 5.2 — VR Educational Experience.

## Requirements

- Unity 2022.3.16f1
- Meta Quest 3, running via PC Link (not standalone)
- Unity XR Interaction Toolkit

## Running it

Open the project in Unity 2022.3.16f1, connect the Quest 3 via PC Link, and enter
Play mode. The controls and task instructions are shown on start and can be
re-shown during the session.

## Notes

The experience targets desktop rendering rather than the headset's mobile GPU,
which removes the frame rate and fidelity limits of standalone deployment. This
matters for an experience whose value depends on maintaining comfort and a
convincing sense of height.
