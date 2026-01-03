# Drone Neutralization Impact Prediction System & Simulation

### Physics-Based Post-Neutralization Risk Modeling and Validation

## Overview

Counter–unmanned aerial system (C-UAS) technologies traditionally focus on detection, tracking, and neutralization. However, neutralization does not eliminate risk. Once propulsion is removed, a drone becomes an uncontrolled falling object whose impact location, timing, and severity depend on its physical state, environmental conditions, and the moment of neutralization.

This project addresses the post-neutralization risk gap by implementing a physics-based simulation system that:

- Predicts post-neutralization descent behavior
- Estimates impact location and horizontal uncertainty
- Computes impact energy and relative risk
- Evaluates whether delaying neutralization could reduce downstream risk
- Validates predictions against actual simulated outcomes

The system is implemented as a modular, extensible MVP, intended for experimentation, training, and future research rather than immediate operational deployment.

## Key Features

- ### Physics-Based Fall Prediction

    - Closed-form kinematic solution for time-to-impact
    - Horizontal motion with wind influence
    - Deterministic, repeatable predictions

- ### Explicit Risk Modeling
    - Impact energy computation
    - Dual risk representation:
        - Continuous normalized risk (risk01)
        - Qualitative risk levels (Low, Medium, High)
    - Heuristic thresholds clearly marked as MVP placeholders

- ### Neutralization Timing Recommendations
    - Evaluates short, bounded delays in neutralization
    - Compares predicted impact energy against immediate neutralization
    - Returns either a delay recommendation or “immediate neutralization”

- ### Spatial Uncertainty Visualization
    - Ground-projected impact disk
    - Radius encodes horizontal drift uncertainty
    - Color opacity encodes relative risk

- ### Validation Against Ground Truth
    - Collision-based impact detection
    - Comparison of predicted vs actual:
        - Time-to-impact
        - Impact energy
        - Horizontal error distance
    - Designed to surface modeling errors, not hide them

## System Architecture
```
Core
 ├─ Physics prediction
 ├─ Risk modeling
 └─ Decision support logic

Infrastructure
 ├─ Unity-based state providers
 ├─ Terrain height provider
 └─ Wind provider

Assembly & Control
 ├─ State assembly
 ├─ Prediction orchestration
 ├─ Neutralization control
 └─ Simulation reset & flow

Presentation
 ├─ UI & telemetry
 ├─ Visualization
 └─ Camera control
```

Prediction logic is intentionally decoupled from Unity components, enabling future replacement of simulation providers with real sensor-driven implementations.

## Neutralization Model (MVP Scope)

- Neutralization is modeled as motor cutoff, resulting in immediate loss of lift and ballistic descent.
- The codebase defines additional neutralization modes (e.g., partial thrust loss, explosive disablement, tethered capture), but these are explicit placeholders and are not yet integrated into the active prediction path.
- No active control, lift, or stabilization is modeled post-neutralization.

## Risk & Recommendation Logic

- Impact energy is used as the primary proxy for risk.
- Risk thresholds are heuristic MVP values, intended for relative comparison and visualization rather than authoritative injury modeling.
- The recommendation engine:
    - Evaluates a discrete, bounded set of short neutralization delays
    - Analytically estimates resulting impact energy
    - Does not perform trajectory planning or continuous optimization

In practice, the system frequently recommends immediate neutralization, which reflects the ballistic assumptions of the MVP rather than a forced design choice.

## Validation Pipeline

A defining feature of the system is explicit, code-driven validation:

- Impact detection is triggered exclusively by physics collision events
- Actual outcomes are measured using the same physical formulations as prediction
- Predicted and actual values are compared immediately after impact
- Validation results are presented visually and numerically
This validation loop directly informed architectural refinements such as centralized state assembly and consistent ground referencing.

## Technologies Used

- Engine: Unity (URP)
- Physics: Unity Rigidbody + custom kinematic modeling
- Visualization: URP DecalProjector, custom shaders
- Architecture: Provider-based design, Clean Architecture principles
- Terrain: Cesium for Unity (real-world terrain integration)

## Intended Use Cases

- C-UAS training and experimentation
- Neutralization timing analysis
- Scenario exploration and what-if analysis
- Research into post-neutralization risk modeling
- Foundation for future physics-ML hybrid approaches

## Limitations (MVP)

- No aerodynamic drag or rotational dynamics
- No glide, lift, or partial-thrust modeling
- Constant wind approximation
- Risk model is intrinsic (energy-based only)
- Not calibrated to real-world injury or damage thresholds

These limitations are explicit and intentional, enabling transparent validation and future extension.

## Future Work

Planned extensions include:

- Neutralization-method–specific descent models
- Higher-fidelity aerodynamics (drag, tumbling)
- Context-aware risk modeling (population, infrastructure)
- Sensor-driven state ingestion
- Hybrid physics–machine learning prediction
- Training and after-action analysis tooling

## Disclaimer

#### This project is a research and simulation prototype.
It is not intended for operational deployment and should not be used for real-world safety or enforcement decisions without further validation and calibration.
