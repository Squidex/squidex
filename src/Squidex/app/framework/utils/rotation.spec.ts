/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Rotation } from './../';

describe('Rotation', () => {
    it('should sub correctly', () => {
        const rotation1 = Rotation.createFromDegree(180);
        const rotation2 = Rotation.createFromDegree(45);

        const actual = rotation1.sub(rotation2);
        const expected = Rotation.createFromDegree(135);

        expect(actual).toEqual(expected);
    });

    it('should add correctly', () => {
        const rotation1 = Rotation.createFromDegree(180);
        const rotation2 = Rotation.createFromDegree(45);

        const actual = rotation1.add(rotation2);
        const expected = Rotation.createFromDegree(225);

        expect(actual).toEqual(expected);
    });

    it('should calculate negated rotation', () => {
        const rotation = Rotation.createFromDegree(180);

        const actual = rotation.negate();
        const expected = Rotation.createFromDegree(-180);

        expect(actual).toEqual(expected);
    });

    it('should create rotation by degree', () => {
        const rotation = Rotation.createFromDegree(180);

        expect(rotation.degree).toBe(180);
        expect(rotation.radian).toBe(Math.PI);

        expect(rotation.cos).toBe(Math.cos(Math.PI));
        expect(rotation.sin).toBe(Math.sin(Math.PI));

        expect(rotation.toString()).toBe('180°');
    });

    it('should create rotation by radian', () => {
        const rotation = Rotation.createFromRadian(Math.PI);

        expect(rotation.degree).toBe(180);
        expect(rotation.radian).toBe(Math.PI);

        expect(rotation.cos).toBe(Math.cos(Math.PI));
        expect(rotation.sin).toBe(Math.sin(Math.PI));

        expect(rotation.toString()).toBe('180°');
    });

    it('should make correct equal comparisons', () => {
        expect(Rotation.createFromDegree(123).eq(Rotation.createFromDegree(123))).toBeTruthy();
        expect(Rotation.createFromDegree(123).eq(Rotation.createFromDegree(234))).toBeFalsy();
    });

    it('should make correct not equal comparisons', () => {
        expect(Rotation.createFromDegree(123).ne(Rotation.createFromDegree(123))).toBeFalsy();
        expect(Rotation.createFromDegree(123).ne(Rotation.createFromDegree(234))).toBeTruthy();
    });
});