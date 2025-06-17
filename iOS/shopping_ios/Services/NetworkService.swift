import Foundation

protocol NetworkServiceProtocol {
    func request<T: Decodable>(
        endpoint: String,
        method: HTTPMethod,
        queryItems: [URLQueryItem]?,
        body: [String: Any]?,
        completion: @escaping (Result<T, NetworkError>) -> Void
    )
    
    func requestWithBearer<T: Decodable>(
        endpoint: String,
        method: HTTPMethod,
        queryItems: [URLQueryItem]?,
        body: [String: Any]?,
        bearerToken: String,
        completion: @escaping (Result<T, NetworkError>) -> Void
    )
}

final class NetworkService: NetworkServiceProtocol {
    static let shared = NetworkService()
    private let baseURL: String

    private init() {
        self.baseURL = "http://sundayti.ru/kpo_3/"
    }

    private func prepareRequest(
        endpoint: String,
        method: HTTPMethod,
        queryItems: [URLQueryItem]?,
        body: [String: Any]?,
        bearer: String?
    ) -> URLRequest? {
        var components = URLComponents(string: baseURL + endpoint)
        components?.queryItems = queryItems
        guard let url = components?.url else { return nil }

        var req = URLRequest(url: url)
        req.httpMethod = method.rawValue
        req.setValue("application/json", forHTTPHeaderField: "Content-Type")
        if let token = bearer {
            req.setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
        }
        if let body = body {
            req.httpBody = try? JSONSerialization.data(
                withJSONObject: body, options: []
            )
        }
        return req
    }

    private func processResponse<T: Decodable>(
        data: Data?,
        response: URLResponse?,
        error: Error?,
        completion: @escaping (Result<T, NetworkError>) -> Void
    ) {
        DispatchQueue.main.async {
            if let err = error {
                completion(.failure(.unknown(message: err.localizedDescription)))
                return
            }
            guard let http = response as? HTTPURLResponse else {
                completion(.failure(.unknown(message: "No HTTP response")))
                return
            }
            guard let data = data else {
                completion(.failure(.noData))
                return
            }
            do {
                let decoded = try JSONDecoder().decode(T.self, from: data)
                if !(200...299).contains(http.statusCode) {
                    let msg = String(data: data, encoding: .utf8) ?? "No message"
                    completion(.failure(.serverError(code: http.statusCode, message: msg)))
                } else {
                    completion(.success(decoded))
                }
            } catch {
                completion(.failure(.decodingError))
            }
        }
    }

    func request<T: Decodable>(
        endpoint: String,
        method: HTTPMethod,
        queryItems: [URLQueryItem]? = nil,
        body: [String: Any]? = nil,
        completion: @escaping (Result<T, NetworkError>) -> Void
    ) {
        guard let req = prepareRequest(
                endpoint: endpoint,
                method: method,
                queryItems: queryItems,
                body: body,
                bearer: nil
        ) else {
            return completion(.failure(.invalidURL))
        }
        URLSession.shared.dataTask(with: req) { data, resp, err in
            self.processResponse(
                data: data,
                response: resp,
                error: err,
                completion: completion
            )
        }.resume()
    }

    func requestWithBearer<T: Decodable>(
        endpoint: String,
        method: HTTPMethod,
        queryItems: [URLQueryItem]? = nil,
        body: [String: Any]? = nil,
        bearerToken: String,
        completion: @escaping (Result<T, NetworkError>) -> Void
    ) {
        guard let req = prepareRequest(
                endpoint: endpoint,
                method: method,
                queryItems: queryItems,
                body: body,
                bearer: bearerToken
        ) else {
            return completion(.failure(.invalidURL))
        }
        URLSession.shared.dataTask(with: req) { data, resp, err in
            self.processResponse(
                data: data,
                response: resp,
                error: err,
                completion: completion
            )
        }.resume()
    }
}
