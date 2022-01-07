/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import MersenneTwister from 'mersenne-twister';

const ALL_COLORS: ReadonlyArray<string> = [
    'rgb(226,27,12)',
    'rgb(192,19,78)',
    'rgb(125,31,141)',
    'rgb(82,46,146)',
    'rgb(50,65,145)',
    'rgb(11,122,209)',
    'rgb(2,135,195)',
    'rgb(0,150,170)',
    'rgb(0,120,109)',
    'rgb(61,140,64)',
    'rgb(112,162,54)',
    'rgb(174,188,33)',
    'rgb(210,157,0)',
    'rgb(204,122,0)',
    'rgb(231,55,0)',
];

const LAYERS = 3;

const RADIUSES: ReadonlyArray<number> = [20, 25, 30, 35, 40, 45, 50];

const X_CENTERS: ReadonlyArray<number> = [0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100];
const Y_CENTERS: ReadonlyArray<number> = [30, 40, 50, 60, 70];

function hash(str: string) {
    if (str.length === 0) {
        return 0;
    }

    let result = 0;

    for (let i = 0; i < str.length; i++) {
        result = result * 31 + str.charCodeAt(i);
        result %= (2 ** 32);
    }

    return result;
}

export function picasso(content: string) {
    const seed = hash(content);
    const rand = new MersenneTwister(seed);

    const colors = [...ALL_COLORS];

    const generateColor = () => {
        const idx = Math.floor(colors.length * rand.random());

        return colors.splice(idx, 1)[0];
    };

    const background = `<rect fill="${generateColor()}" width="100" height="100"/>`;

    let shape = '';

    for (let i = 0; i < LAYERS; i++) {
        const rd = RADIUSES[Math.floor(RADIUSES.length * rand.random())];

        const cx = X_CENTERS[Math.floor(X_CENTERS.length * rand.random())];
        const cy = Y_CENTERS[Math.floor(Y_CENTERS.length * rand.random())];

        const fill = generateColor();

        shape += `<circle r="${rd}" cx="${cx}" cy="${cy}" fill="${fill}" opacity="0."/>`;
    }

    return `<svg version="1.1" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100">${background}${shape}</svg>`;
}
