# ATHLO Mobile

React Native (Expo Router) client for the ATHLO fitness platform.

## Prerequisites

- Node.js **20.19.4+** (or 22.13+, 24.3+) — React Native 0.85 requires at least 20.19.4. With [nvm](https://github.com/nvm-sh/nvm): `nvm install` in this folder (uses `.nvmrc`).
- Backend services running (see [../README.md](../README.md))
- Expo Go app or Android/iOS simulator

## Setup

```bash
cd Frontend
npm install
cp .env.example .env
# Edit .env — set API URLs for your environment
```

### API URLs

| Environment | Auth API | Management API |
|-------------|----------|----------------|
| iOS Simulator | `http://localhost:5001` | `http://localhost:5000` |
| Android Emulator | `http://10.0.2.2:5001` | `http://10.0.2.2:5000` |
| Physical device | Your machine's LAN IP | Your machine's LAN IP |

## Run

```bash
npm start
# then press i (iOS), a (Android), or w (web)
```

## Features

- **Auth** — login, register, forgot/reset password
- **Programs** — browse catalog, view exercises, start workout
- **Active workout** — timer, complete or cancel
- **Progress** — stats, recent workouts, goal tracking
- **Profile** — user info, sign out

## Project structure

```
app/           # Expo Router screens
components/    # UI components
lib/api/       # API clients (axios + token refresh)
lib/types/     # TypeScript types matching backend DTOs
stores/        # Zustand auth store + SecureStore
constants/     # Theme tokens
```
