import axios from 'axios';

const axiosInstance = axios.create({
    baseURL: 'http://localhost:9090',
    timeout: 10000
});

axiosInstance.interceptors.request.use(config => {
    const token = localStorage.getItem('jwtToken');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
}, error => {
    return Promise.reject(error);
});

export default axiosInstance;