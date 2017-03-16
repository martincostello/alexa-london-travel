#!/bin/sh
npm run lint || exit 1
npm test || exit 1
