/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computeAnchors, positionModal, PositionRequest, SimplePosition } from './modal-positioner';

describe('position', () => {
    function buildRect(x: number, y: number, w: number, h: number): any {
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

    const tests: { position: SimplePosition; x: number; y: number }[] = [
        { position: 'bottom-center', x: 235, y: 310 },
        { position: 'bottom-left', x: 210, y: 310 },
        { position: 'bottom-right', x: 260, y: 310 },
        { position: 'left-bottom', x: 160, y: 260 },
        { position: 'left-center', x: 160, y: 235 },
        { position: 'left-top', x: 160, y: 210 },
        { position: 'right-bottom', x: 310, y: 260 },
        { position: 'right-center', x: 310, y: 235 },
        { position: 'right-top', x: 310, y: 210 },
        { position: 'top-center', x: 235, y: 160 },
        { position: 'top-left', x: 210, y: 160 },
        { position: 'top-right', x: 260, y: 160 },
    ];

    tests.forEach(test => {
        it(`should calculate modal position for ${test.position}`, () => {
            const modalRect = buildRect(0, 0, 30, 30);

            const [anchorX, anchorY] = computeAnchors(test.position);

            const request: PositionRequest = {
                anchorX,
                anchorY,
                clientHeight: 1000,
                clientWidth: 1000,
                offsetX: 10,
                offsetY: 10,
                modalRect,
                targetRect,
            };

            const result = positionModal(request);

            expect(result.x).toBe(test.x);
            expect(result.y).toBe(test.y);
        });
    });

    it('should calculate modal position for vertical top fix', () => {
        const modalRect = buildRect(0, 0, 30, 200);

        const [anchorX, anchorY] = computeAnchors('top-left');

        const request: PositionRequest = {
            adjust: true,
            anchorX,
            anchorY,
            clientHeight: 600,
            clientWidth: 600,
            modalRect,
            offsetX: 10,
            offsetY: 10,
            targetRect,
        };

        const result = positionModal(request);

        expect(result.x).toBe(210);
        expect(result.y).toBe(310);
    });

    it('should calculate modal position for vertical bottom fix', () => {
        const modalRect = buildRect(0, 0, 30, 70);

        const [anchorX, anchorY] = computeAnchors('bottom-left');

        const request: PositionRequest = {
            adjust: true,
            anchorX,
            anchorY,
            clientHeight: 350,
            clientWidth: 350,
            modalRect,
            offsetX: 10,
            offsetY: 10,
            targetRect,
        };

        const result = positionModal(request);

        expect(result.x).toBe(210);
        expect(result.y).toBe(120);
    });

    it('should calculate modal position for horizontal left fix', () => {
        const modalRect = buildRect(0, 0, 200, 30);

        const [anchorX, anchorY] = computeAnchors('left-top');

        const request: PositionRequest = {
            adjust: true,
            anchorX,
            anchorY,
            clientHeight: 600,
            clientWidth: 600,
            modalRect,
            offsetX: 10,
            offsetY: 10,
            targetRect,
        };

        const result = positionModal(request);

        expect(result.x).toBe(310);
        expect(result.y).toBe(210);
    });

    it('should calculate modal position for horizontal right fix', () => {
        const modalRect = buildRect(0, 0, 70, 30);

        const [anchorX, anchorY] = computeAnchors('right-top');

        const request: PositionRequest = {
            adjust: true,
            anchorX,
            anchorY,
            clientHeight: 350,
            clientWidth: 350,
            modalRect,
            offsetX: 10,
            offsetY: 10,
            targetRect,
        };

        const result = positionModal(request);

        expect(result.x).toBe(120);
        expect(result.y).toBe(210);
    });
});
