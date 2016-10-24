/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { MathHelper } from './math-helper';

export class Rotation {
    public static readonly ZERO = Rotation.createFromRadian(0);

    public cos: number;
    public sin: number;

    constructor(
        public readonly radian: number,
        public readonly degree: number
    ) {
        this.cos = Math.cos(radian);
        this.sin = Math.sin(radian);

        Object.freeze(this);
    }

    public static createFromRadian(radian: number): Rotation {
        return new Rotation(radian, MathHelper.toDegree(radian));
    }

    public static createFromDegree(degree: number): Rotation {
        return new Rotation(MathHelper.toRad(degree), degree);
    }

    public eq(r: Rotation): boolean {
        return this.radian === r.radian;
    }

    public ne(r: Rotation): boolean {
        return this.radian !== r.radian;
    }

    public toString(): string {
        return `${this.degree}°`;
    }

    public add(r: Rotation): Rotation {
        return Rotation.createFromDegree(MathHelper.toPositiveDegree(this.degree + r.degree));
    }

    public sub(r: Rotation): Rotation {
        return Rotation.createFromDegree(MathHelper.toPositiveDegree(this.degree - r.degree));
    }

    public negate(): Rotation {
        return Rotation.createFromDegree(-this.degree);
    }
}