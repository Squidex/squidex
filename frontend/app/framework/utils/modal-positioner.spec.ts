/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { positionModal } from './modal-positioner';

describe('position', () => {
    function buildRect(x: number, y: number, w: number, h: number): ClientRect {
        return {
            top: y,
            left: x,
            right: x + w,
            width: w,
            height: h,
            bottom: y + h,
        };
    }

    const targetRect = buildRect(200, 200, 100, 100);

    const tests = [
        { position: 'top', x: 235, y: 160 },
        { position: 'top-left', x: 200, y: 160 },
        { position: 'top-right', x: 270, y: 160 },
        { position: 'bottom', x: 235, y: 310 },
        { position: 'bottom-left', x: 200, y: 310 },
        { position: 'bottom-right', x: 270, y: 310 },
        { position: 'left', x: 160, y: 235 },
        { position: 'left-top', x: 160, y: 200 },
        { position: 'left-bottom', x: 160, y: 270 },
        { position: 'right', x: 310, y: 235 },
        { position: 'right-top', x: 310, y: 200 },
        { position: 'right-bottom', x: 310, y: 270 },
    ];

    tests.forEach(test => {
        it(`should calculate modal position for ${test.position}`, () => {
            const modalRect = buildRect(0, 0, 30, 30);

            const result = positionModal(targetRect, modalRect, test.position, 10, false, 0, 0);

            expect(result.x).toBe(test.x);
            expect(result.y).toBe(test.y);
        });
    });

    it('should calculate modal position for vertical top fix', () => {
        const modalRect = buildRect(0, 0, 30, 200);

        const result = positionModal(targetRect, modalRect, 'top-left', 10, true, 600, 600);

        expect(result.x).toBe(200);
        expect(result.y).toBe(310);
    });

    it('should calculate modal position for vertical bottom fix', () => {
        const modalRect = buildRect(0, 0, 30, 70);

        const result = positionModal(targetRect, modalRect, 'bottom-left', 10, true, 350, 350);

        expect(result.x).toBe(200);
        expect(result.y).toBe(120);
    });

    it('should calculate modal position for horizontal left fix', () => {
        const modalRect = buildRect(0, 0, 200, 30);

        const result = positionModal(targetRect, modalRect, 'left-top', 10, true, 600, 600);

        expect(result.x).toBe(310);
        expect(result.y).toBe(200);
    });

    it('should calculate modal position for horizontal right fix', () => {
        const modalRect = buildRect(0, 0, 70, 30);

        const result = positionModal(targetRect, modalRect, 'right-top', 10, true, 350, 350);

        expect(result.x).toBe(120);
        expect(result.y).toBe(200);
    });
});
