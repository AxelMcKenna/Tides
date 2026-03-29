// swift-tools-version: 5.10
import PackageDescription

let package = Package(
    name: "ShoreKit",
    platforms: [.iOS(.v17), .macOS(.v14)],
    products: [
        .library(name: "ShoreKit", targets: ["ShoreKit"]),
    ],
    dependencies: [
        .package(url: "https://github.com/groue/GRDB.swift.git", from: "7.0.0"),
    ],
    targets: [
        .target(
            name: "ShoreKit",
            dependencies: [
                .product(name: "GRDB", package: "GRDB.swift"),
            ]
        ),
        .testTarget(
            name: "ShoreKitTests",
            dependencies: ["ShoreKit"]
        ),
    ]
)
