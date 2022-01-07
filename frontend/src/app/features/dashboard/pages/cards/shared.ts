/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DateTime } from '@app/shared';

const ColorSchema: ReadonlyArray<string> = [
    ' 51, 137, 213',
    '211,  50,  50',
    '131, 211,  50',
    ' 50, 211, 131',
    ' 50, 211, 211',
    ' 50, 131, 211',
    ' 50,  50, 211',
    ' 50, 211,  50',
    '131,  50, 211',
    '211,  50, 211',
    '211,  50, 131',
];

export module ChartHelpers {
    export function label(category: string) {
        return category === '*' ? 'anonymous' : category;
    }

    export function createLabels(dtos: ReadonlyArray<{ date: DateTime }>): ReadonlyArray<string> {
        return dtos.map(d => d.date.toStringFormat('M-dd'));
    }

    export function createLabelsFromSet(dtos: { [category: string]: ReadonlyArray<{ date: DateTime }> }): ReadonlyArray<string> {
        return createLabels(dtos[Object.keys(dtos)[0]]);
    }

    export function getBackgroundColor(i = 0) {
        return `rgba(${ColorSchema[i]}, 0.6)`;
    }

    export function getBorderColor(i = 0) {
        return `rgba(${ColorSchema[i]}, 1)`;
    }
}

export module ChartOptions {
    export const Default = {
        responsive: true,
        scales: {
            xAxes: [{
                display: true,
                stacked: false,
            }],
            yAxes: [{
                ticks: {
                    beginAtZero: true,
                },
                stacked: false,
            }],
        },
        maintainAspectRatio: false,
    };

    export const Stacked = {
        responsive: true,
        scales: {
            xAxes: [{
                display: true,
                stacked: true,
            }],
            yAxes: [{
                ticks: {
                    beginAtZero: true,
                },
                stacked: true,
            }],
        },
        maintainAspectRatio: false,
    };
}
