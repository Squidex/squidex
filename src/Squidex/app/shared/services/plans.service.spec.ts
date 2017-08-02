/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    AppPlansDto,
    ApiUrlConfig,
    ChangePlanDto,
    PlanChangedDto,
    PlanDto,
    PlansService,
    Version
} from './../';

describe('PlansService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                PlansService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get app plans',
        inject([PlansService, HttpTestingController], (plansService: PlansService, httpMock: HttpTestingController) => {

        let plans: AppPlansDto | null = null;

        plansService.getPlans('my-app', version).subscribe(result => {
            plans = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/plans');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({
            currentPlanId: '123',
            hasPortal: true,
            planOwner: '456',
            plans: [
                {
                    id: 'free',
                    name: 'Free',
                    costs: '14 €',
                    maxApiCalls: 1000,
                    maxAssetSize: 1500,
                    maxContributors: 2500
                },
                {
                    id: 'prof',
                    name: 'Prof',
                    costs: '18 €',
                    maxApiCalls: 4000,
                    maxAssetSize: 5500,
                    maxContributors: 6500
                }
            ]
        });

        expect(plans).toEqual(
            new AppPlansDto(
                '123',
                '456',
                true,
                [
                    new PlanDto('free', 'Free', '14 €', 1000, 1500, 2500),
                    new PlanDto('prof', 'Prof', '18 €', 4000, 5500, 6500)
                ]
            ));
    }));

    it('should make put request to change plan',
        inject([PlansService, HttpTestingController], (plansService: PlansService, httpMock: HttpTestingController) => {

        const dto = new ChangePlanDto('enterprise');

        let planChanged: PlanChangedDto | null = null;

        plansService.putPlan('my-app', dto, version).subscribe(result => {
            planChanged = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/plan');

        req.flush({ redirectUri: 'my-url' });

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        expect(planChanged).toEqual(new PlanChangedDto('my-url'));
    }));
});