import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    vus: 500,
    duration: '2m',
    insecureSkipTLSVerify: true,
};

export default function () {
    const url = 'http://127.0.0.1:5087/api/exams/sim-nocache?alias=danh-gia-nang-luc-toan-hoc';
    const res = http.get(url);
    
    check(res, {
        'status is 200': (r) => r.status === 200,
    });
    
    sleep(0.1);
}
