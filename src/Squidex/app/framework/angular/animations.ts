/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import {
    animate,
    AnimationEntryMetadata,
    state,
    style,
    transition,
    trigger
} from '@angular/core';

export function buildSlideRightAnimation(name = 'slideRight', timing = '150ms'): AnimationEntryMetadata {
    return trigger(
        name, [
            transition(':enter', [
                style({ transform: 'translateX(-100%)' }),
                animate(timing, style({ transform: 'translateX(0%)' }))
            ]),
            transition(':leave', [
                style({transform: 'translateX(0%)' }),
                animate(timing, style({ transform: 'translateX(-100%)' }))
            ]),
            state('true',
                style({ transform: 'translateX(0%)' })
            ),
            state('false',
                style({ transform: 'translateX(-100%)' })
            ),
            transition('1 => 0', animate(timing)),
            transition('0 => 1', animate(timing))
        ]
    );
}

export function buildSlideAnimation(name = 'slide', timing = '400ms'): AnimationEntryMetadata {
    return trigger(
        name, [
            transition(':enter', [
                style({ transform: 'translateX(100%)' }),
                animate(timing, style({ transform: 'translateX(0%)' }))
            ]),
            transition(':leave', [
                style({transform: 'translateX(0%)' }),
                animate(timing, style({ transform: 'translateX(-100%)' }))
            ]),
            state('true',
                style({ transform: 'translateX(0%)' })
            ),
            state('false',
                style({ transform: 'translateX(-100%)' })
            ),
            transition('1 => 0', animate(timing)),
            transition('0 => 1', animate(timing))
        ]
    );
}

export function buildFadeAnimation(name = 'fade', timing = '150ms'): AnimationEntryMetadata {
    return trigger(
        name, [
            transition(':enter', [
                style({ opacity: 0 }),
                animate(timing, style({ opacity: 1 }))
            ]),
            transition(':leave', [
                style({ opacity: 1 }),
                animate(timing, style({ opacity: 0 }))
            ]),
            state('true',
                style({ opacity: 1 })
            ),
            state('false',
                style({ opacity: 0 })
            ),
            transition('1 => 0', animate(timing)),
            transition('0 => 1', animate(timing))
        ]
    );
};

export function buildHeightAnimation(name = 'height', timing = '200ms'): AnimationEntryMetadata {
    return trigger(
        name, [
            transition(':enter', [
                style({ height: '0px' }),
                animate(timing, style({ height: '*' }))
            ]),
            transition(':leave', [
                style({ height: '*' }),
                animate(timing, style({ height: '0px' }))
            ]),
            state('true',
                style({ height: '*' })
            ),
            state('false',
                style({ height: '0px' })
            ),
            transition('1 => 0', animate(timing)),
            transition('0 => 1', animate(timing))
        ]
    );
};

export const fadeAnimation = buildFadeAnimation();
export const heightAnimation = buildHeightAnimation();
export const slideAnimation = buildSlideAnimation();
export const slideRightAnimation = buildSlideRightAnimation();
