/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { AnalyticsService, ApiUrlConfig, PlanChangedDto, PlanDto, PlansDto, PlansService, Version } from '@app/shared/internal';

describe('PlansService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
                PlansService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                { provide: AnalyticsService, useValue: new AnalyticsService() },
            ],
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get app plans',
        inject([PlansService, HttpTestingController], (plansService: PlansService, httpMock: HttpTestingController) => {
            let plans: PlansDto;

            plansService.getPlans('my-app').subscribe(result => {
                plans = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/plans');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                currentPlanId: '123',
                planOwner: '456',
                plans: [
                    {
                        id: 'free',
                        name: 'Free',
                        costs: '14 €',
                        confirmText: 'Change for 14 € per month?',
                        yearlyId: 'free_yearly',
                        yearlyCosts: '120 €',
                        yearlyConfirmText: 'Change for 120 € per year?',
                        maxApiBytes: 128,
                        maxApiCalls: 1000,
                        maxAssetSize: 1500,
                        maxContributors: 2500,
                    },
                    {
                        id: 'professional',
                        name: 'Professional',
                        costs: '18 €',
                        confirmText: 'Change for 18 € per month?',
                        yearlyId: 'professional_yearly',
                        yearlyCosts: '160 €',
                        yearlyConfirmText: 'Change for 160 € per year?',
                        maxApiBytes: 512,
                        maxApiCalls: 4000,
                        maxAssetSize: 5500,
                        maxContributors: 6500,
                    },
                ],
                hasPortal: true,
            }, {
                headers: {
                    etag: '2',
                },
            });

            expect(plans!).toEqual({
                payload: {
                    currentPlanId: '123',
                    planOwner: '456',
                    plans: [
                        new PlanDto(
                            'free', 'Free', '14 €',
                            'Change for 14 € per month?',
                            'free_yearly', '120 €',
                            'Change for 120 € per year?',
                            128, 1000, 1500, 2500),
                        new PlanDto(
                            'professional', 'Professional', '18 €',
                            'Change for 18 € per month?',
                            'professional_yearly', '160 €',
                            'Change for 160 € per year?',
                            512, 4000, 5500, 6500),
                    ],
                    hasPortal: true,
                },
                version: new Version('2'),
            });
        }));

    it('should make put request to change plan',
        inject([PlansService, HttpTestingController], (plansService: PlansService, httpMock: HttpTestingController) => {
            const dto = { planId: 'enterprise' };

            let planChanged: PlanChangedDto;

            plansService.putPlan('my-app', dto, version).subscribe(result => {
                planChanged = result.payload;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/plan');

            req.flush({ redirectUri: 'http://url' });

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            expect(planChanged!).toEqual({ redirectUri: 'http://url' });
        }));
});
